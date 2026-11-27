using PersistentEmpires.Views.ViewsVM;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using System.Collections.Generic;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace PersistentEmpires.Views.Views
{
    public sealed class PlayerVoiceData
    {
        private DateTime _lastVoicePacketUtc;
        private bool _hasReceivedVoicePacket;
        public NetworkCommunicator Player { get; private set; }
        public int VoiceEventIndex { get; private set; }
        public int RecordedMs { get; set; }

        // Kept as a property because PEAgentLabelUI uses it to show the speaking indicator.
        public bool HasVoiceData
        {
            get { return _hasReceivedVoicePacket && (DateTime.UtcNow - _lastVoicePacketUtc).TotalMilliseconds < 350.0; }
        }

        public PlayerVoiceData(NetworkCommunicator player, int voiceEventIndex)
        {
            Player = player;
            VoiceEventIndex = voiceEventIndex;
            _lastVoicePacketUtc = DateTime.MinValue;
        }

        public void MarkVoicePacketReceived()
        {
            _lastVoicePacketUtc = DateTime.UtcNow;
            _hasReceivedVoicePacket = true;
        }
    }

    public class PEProximityChatView : MissionView
    {
        private const int MaxEncodedPacketSize = 1440;
        private const int MaxPacketsPerBatch = 10;
        private const int EncoderClientId = 0;
        private ProximityChatComponent _proximityChatComponent;
        private bool _isActive;
        private bool _isRecording;
        private bool _voiceChatEnabled = true;
        private bool _voicePlayEventsInitialized;
        private int _nextVoiceEventIndex;
        private float _inputGain = 1f;
        private readonly Dictionary<NetworkCommunicator, PlayerVoiceData> _playerVoiceData = new Dictionary<NetworkCommunicator, PlayerVoiceData>();
        private GauntletLayer _gauntletLayer;
        private PEVoiceChatOptionsVM _dataSource;
        // Native Bannerlord voice capture uses the raw voice-frame size expected by its engine.
        private readonly byte[] _rawVoiceBuffer = new byte[VoiceChatHandler.VoiceFrameRawSizeInBytes];
        public delegate void HandlePeerVoiceStatusUpdated(PlayerVoiceData playerVoiceData, NetworkCommunicator player);
        public event HandlePeerVoiceStatusUpdated OnPeerVoiceStatusUpdated;

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();

            var registerer = new GameNetwork.NetworkMessageHandlerRegisterer(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
            registerer.RegisterBaseHandler<EnableVoiceChat>(new GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>(HandleEnableVoiceChat));
        }

        public override void OnRemoveBehavior()
        {
            StopRecording();
            DetachFromProximityChat();
            DestroyVoicePlayback();

            var registerer = new GameNetwork.NetworkMessageHandlerRegisterer(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
            registerer.RegisterBaseHandler<EnableVoiceChat>(new GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>(HandleEnableVoiceChat));

            base.OnRemoveBehavior();
        }

        private void HandleEnableVoiceChat(GameNetworkMessage networkMessage)
        {
            EnableVoiceChat message = networkMessage as EnableVoiceChat;
            if (message == null)
                return;

            _voiceChatEnabled = message.Enabled;
            if (!message.Enabled)
            {
                StopRecording();
                _isActive = false;
                return;
            }

            _proximityChatComponent = Mission.GetMissionBehavior<ProximityChatComponent>();
            if (_proximityChatComponent == null)
                return;

            AttachToProximityChat();

            try
            {
                InitializeVoicePlayback();
                _isActive = true;
            }
            catch (Exception exception)
            {
                _isActive = false;
                _voiceChatEnabled = false;
                InformationManager.DisplayMessage(new InformationMessage("Bannerlord native voice initialization failed: " + exception.Message,Color.ConvertStringToColor("#FF0000FF")));

                return;
            }

            _dataSource = new PEVoiceChatOptionsVM(_voiceChatEnabled, CloseOptions, ApplyOptions);
        }

        private void AttachToProximityChat()
        {
            if (_proximityChatComponent == null)
                return;

            _proximityChatComponent.OnVoicePlayMessage -= HandleVoicePlayMessage;
            _proximityChatComponent.OnVoicePlayerJoined -= HandleVoicePlayerJoined;
            _proximityChatComponent.OnVoicePlayerLeaved -= HandleVoicePlayerLeaved;
            _proximityChatComponent.OnOptionsClicked -= HandleOptionsClicked;
            _proximityChatComponent.OnVoicePlayMessage += HandleVoicePlayMessage;
            _proximityChatComponent.OnVoicePlayerJoined += HandleVoicePlayerJoined;
            _proximityChatComponent.OnVoicePlayerLeaved += HandleVoicePlayerLeaved;
            _proximityChatComponent.OnOptionsClicked += HandleOptionsClicked;
        }

        private void DetachFromProximityChat()
        {
            if (_proximityChatComponent == null)
                return;

            _proximityChatComponent.OnVoicePlayMessage -= HandleVoicePlayMessage;
            _proximityChatComponent.OnVoicePlayerJoined -= HandleVoicePlayerJoined;
            _proximityChatComponent.OnVoicePlayerLeaved -= HandleVoicePlayerLeaved;
            _proximityChatComponent.OnOptionsClicked -= HandleOptionsClicked;
            _proximityChatComponent = null;
        }

        private void InitializeVoicePlayback()
        {
            if (!_voicePlayEventsInitialized)
            {
                SoundManager.InitializeVoicePlayEvent();
                _voicePlayEventsInitialized = true;
                _nextVoiceEventIndex = 0;
            }

            foreach (NetworkCommunicator peer in GameNetwork.NetworkPeers)
            {
                if (!_playerVoiceData.ContainsKey(peer))
                    AddVoicePlayer(peer);
            }
        }

        private void AddVoicePlayer(NetworkCommunicator peer)
        {
            if (peer == null || _playerVoiceData.ContainsKey(peer) || !_voicePlayEventsInitialized)
                return;

            int voiceEventIndex = _nextVoiceEventIndex++;
            SoundManager.CreateVoiceEvent();
            _playerVoiceData.Add(peer, new PlayerVoiceData(peer, voiceEventIndex));
        }

        private void RemoveVoicePlayer(NetworkCommunicator peer)
        {
            PlayerVoiceData voiceData;
            if (peer == null || !_playerVoiceData.TryGetValue(peer, out voiceData))
                return;

            _playerVoiceData.Remove(peer);
            if (_voicePlayEventsInitialized)
                SoundManager.DestroyVoiceEvent(voiceData.VoiceEventIndex);
        }

        private void DestroyVoicePlayback()
        {
            if (!_voicePlayEventsInitialized)
            {
                _playerVoiceData.Clear();
                return;
            }

            foreach (PlayerVoiceData voiceData in _playerVoiceData.Values)
                SoundManager.DestroyVoiceEvent(voiceData.VoiceEventIndex);

            _playerVoiceData.Clear();
            SoundManager.FinalizeVoicePlayEvent();
            _voicePlayEventsInitialized = false;
            _nextVoiceEventIndex = 0;
        }

        private void HandleVoicePlayerJoined(NetworkCommunicator player)
        {
            AddVoicePlayer(player);
        }

        private void HandleVoicePlayerLeaved(NetworkCommunicator player)
        {
            RemoveVoicePlayer(player);
        }

        private void HandleVoicePlayMessage(SendBatchVoiceToPlay message)
        {
            if (!_isActive || !_voiceChatEnabled || message == null || message.Peer == null)
                return;

            PlayerVoiceData voiceData;
            if (!_playerVoiceData.TryGetValue(message.Peer, out voiceData))
                return;

            MissionPeer missionPeer = message.Peer.GetComponent<MissionPeer>();
            if (missionPeer != null && missionPeer.IsMutedFromGameOrPlatform)
                return;

            int sourceOffset = 0;
            for (int i = 0; i < message.BufferLens.Length; i++)
            {
                int packetLength = message.BufferLens[i];
                if (packetLength <= 0 || packetLength > MaxEncodedPacketSize ||
                    sourceOffset + packetLength > message.PackedBuffer.Length)
                    break;

                byte[] packet = new byte[packetLength];
                Buffer.BlockCopy(message.PackedBuffer, sourceOffset, packet, 0, packetLength);
                sourceOffset += packetLength;

                // Bannerlord performs native Opus decoding and playback for this voice event.
                SoundManager.UpdateVoiceToPlay(packet, packetLength, voiceData.VoiceEventIndex);
                voiceData.MarkVoicePacketReceived();

                if (OnPeerVoiceStatusUpdated != null)
                    OnPeerVoiceStatusUpdated(voiceData, message.Peer);
            }
        }

        public override void OnPreMissionTick(float dt)
        {
            base.OnPreMissionTick(dt);

            if (!_isActive || !_voiceChatEnabled || !_isRecording || _proximityChatComponent == null)
                return;

            ReadAndSendVoiceFrames();
        }

        private void ReadAndSendVoiceFrames()
        {
            var encodedPackets = new List<byte[]>(MaxPacketsPerBatch);
            var packetLengths = new List<int>(MaxPacketsPerBatch);

            for (int i = 0; i < MaxPacketsPerBatch; i++)
            {
                int readBytesLength;
                SoundManager.GetVoiceData(_rawVoiceBuffer, _rawVoiceBuffer.Length, out readBytesLength);

                if (readBytesLength <= 0)
                    break;

                if (readBytesLength > _rawVoiceBuffer.Length)
                    readBytesLength = _rawVoiceBuffer.Length;

                ApplyInputGain(_rawVoiceBuffer, readBytesLength, _inputGain);

                byte[] compressedBuffer = new byte[MaxEncodedPacketSize];
                int compressedLength;
                SoundManager.CompressData(EncoderClientId, _rawVoiceBuffer, readBytesLength,
                    compressedBuffer, out compressedLength);

                if (compressedLength <= 0 || compressedLength > MaxEncodedPacketSize)
                    continue;

                encodedPackets.Add(compressedBuffer);
                packetLengths.Add(compressedLength);
            }

            if (packetLengths.Count > 0)
                _proximityChatComponent.SendEncodedVoiceToServer(encodedPackets.ToArray(), packetLengths.ToArray());
        }

        private static void ApplyInputGain(byte[] pcmBuffer, int length, float gain)
        {
            if (Math.Abs(gain - 1f) < 0.001f)
                return;

            // Bannerlord's voice capture buffer is signed 16-bit PCM.
            for (int i = 0; i + 1 < length; i += 2)
            {
                short sample = (short)(pcmBuffer[i] | (pcmBuffer[i + 1] << 8));
                int scaled = (int)(sample * gain);
                if (scaled > short.MaxValue) scaled = short.MaxValue;
                if (scaled < short.MinValue) scaled = short.MinValue;
                pcmBuffer[i] = (byte)(scaled & 0xFF);
                pcmBuffer[i + 1] = (byte)((scaled >> 8) & 0xFF);
            }
        }

        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);

            if (!_isActive || !_voiceChatEnabled)
                return;

            try
            {
                if (Input.IsKeyPressed(TaleWorlds.InputSystem.InputKey.V))
                    StartRecording();

                if (Input.IsKeyReleased(TaleWorlds.InputSystem.InputKey.V))
                    StopRecording();
            }
            catch (InvalidOperationException exception)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "Bannerlord voice recording error: " + exception.Message,
                    Color.ConvertStringToColor("#FF0000FF")));
                StopRecording();
            }
        }

        private void StartRecording()
        {
            if (_isRecording || !_isActive || !_voiceChatEnabled)
                return;

            SoundManager.StartVoiceRecording();
            _isRecording = true;
        }

        private void StopRecording()
        {
            if (!_isRecording)
                return;

            _isRecording = false;
            SoundManager.StopVoiceRecording();
            // Drain any final captured frame before the native recorder is stopped.
            if (_isActive && _voiceChatEnabled && _proximityChatComponent != null)
                ReadAndSendVoiceFrames();
        }

        private void ApplyOptions(bool voiceChatEnabled, int inputGain)
        {
            _voiceChatEnabled = voiceChatEnabled;
            _inputGain = Math.Max(0, Math.Min(200, inputGain)) / 100f;

            if (!_voiceChatEnabled)
                StopRecording();
        }

        private void HandleOptionsClicked()
        {
            if (_dataSource != null)
                OpenOptionsMenu();
        }

        private void OpenOptionsMenu()
        {
            if (MissionScreen == null || _dataSource == null)
                return;

            _gauntletLayer = new GauntletLayer("PEProximityChatView", 2);
            _gauntletLayer.LoadMovie("PEVoiceChatOptions", _dataSource);
            _gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.Mouse);
            MissionScreen.AddLayer(_gauntletLayer);
        }

        private void CloseOptions()
        {
            if (_gauntletLayer == null || MissionScreen == null)
                return;

            _gauntletLayer.InputRestrictions.ResetInputRestrictions();
            MissionScreen.RemoveLayer(_gauntletLayer);
            _gauntletLayer = null;
        }

        public override void OnMissionScreenFinalize()
        {
            StopRecording();
            DetachFromProximityChat();
            DestroyVoicePlayback();
            _isActive = false;
            base.OnMissionScreenFinalize();
        }
    }
}

//using Concentus.Structs;
//using NAudio;
//using NAudio.Wave;
//using NAudio.Wave.SampleProviders;
//using PersistentEmpires.Views.ViewsVM;
//using PersistentEmpiresLib.NetworkMessages.Server;
//using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using TaleWorlds.Core;
//using TaleWorlds.Engine.GauntletUI;
//using TaleWorlds.Library;
//using TaleWorlds.MountAndBlade;
//using TaleWorlds.MountAndBlade.Network.Messages;
//using TaleWorlds.MountAndBlade.View.MissionViews;

//namespace PersistentEmpires.Views.Views
//{
//    public class PlayerVoiceData
//    {
//        public NetworkCommunicator Player;
//        public BufferedWaveProvider InputWaveProvider;
//        public VolumeWaveProvider16 VolumeSampleProvider;
//        public OpusDecoder OpusDecoder;
//        public int RecordedMs;
//        public bool HasVoiceData;
//        public PlayerVoiceData(NetworkCommunicator player)
//        {
//            this.Player = player;
//            this.InputWaveProvider = new BufferedWaveProvider(new WaveFormat(48000, 16, 1));
//            this.VolumeSampleProvider = new VolumeWaveProvider16(this.InputWaveProvider);
//            this.OpusDecoder = new OpusDecoder(48000, 1);
//            this.RecordedMs = 0;
//        }

//        public void AddSample(byte[] outputByte, int offset, int length)
//        {
//            try
//            {
//                InputWaveProvider.AddSamples(outputByte, offset, length);
//            }
//            catch (Exception e)
//            {
//                InputWaveProvider.ClearBuffer();
//            }
//        }

//        public byte[] ReadSample(int size)
//        {
//            byte[] readedByte = new byte[size * 2];
//            this.VolumeSampleProvider.Read(readedByte, 0, size * 2);
//            return readedByte;
//        }
//    }
//    public class PEProximityChatView : MissionView
//    {
//        private ProximityChatComponent _proximityChatComponent;
//        private bool IsActive = false;
//        private bool IsRecording = false;
//        private bool SelfRecording = false;
//        private bool VoiceChatEnabled = true;
//        private OpusEncoder opusEncoder;
//        private float inputGain = 1f;
//        // private OpusDecoder opusDecoder;
//        private WaveInEvent sourceStream;
//        private WaveOutEvent outputStream;
//        private BufferedWaveProvider bufferedWaveProvider;
//        private VolumeSampleProvider outputVolume;
//        private Dictionary<NetworkCommunicator, PlayerVoiceData> _playerVoiceData = new Dictionary<NetworkCommunicator, PlayerVoiceData>();

//        private List<NetworkCommunicator> _tempHaveData = new List<NetworkCommunicator>();
//        private BufferedWaveProvider selfBufferedWaveProvider;
//        private WaveOutEvent selfOutputStream;
//        private GauntletLayer _gauntletLayer;
//        private PEVoiceChatOptionsVM _dataSource;

//        public delegate void HandlePeerVoiceStatusUpdated(PlayerVoiceData playerVoiceData, NetworkCommunicator player);
//        public event HandlePeerVoiceStatusUpdated OnPeerVoiceStatusUpdated;

//        private int bufferMs = 180;

//        private void InitializeVoiceChatComponents()
//        {
//            opusEncoder = new OpusEncoder(48000, 1, Concentus.Enums.OpusApplication.OPUS_APPLICATION_VOIP);
//            opusEncoder.Bitrate = 16 * 1024;

//            // opusDecoder = OpusDecoder.Create(48000, 1);

//            sourceStream = new WaveInEvent();
//            sourceStream.BufferMilliseconds = bufferMs;
//            sourceStream.NumberOfBuffers = 1;
//            sourceStream.WaveFormat = new WaveFormat(48000, 16, 1);

//            sourceStream.DataAvailable += new EventHandler<WaveInEventArgs>(OnVoiceRecord);
//            // sourceStream.StartRecording();

//            selfBufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(48000, 16, 1));
//            selfOutputStream = new WaveOutEvent();
//            selfOutputStream.Init(selfBufferedWaveProvider);

//            foreach (NetworkCommunicator peer in GameNetwork.NetworkPeers)
//            {
//                this._playerVoiceData[peer] = new PlayerVoiceData(peer);
//            }

//            outputStream = new WaveOutEvent();

//            bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(48000, 16, 1));
//            outputVolume = new VolumeSampleProvider(bufferedWaveProvider.ToSampleProvider());
//            outputStream.Init(outputVolume);
//            outputStream.Play();


//            // selfOutputStream.Play();


//        }

//        public override void OnBehaviorInitialize()
//        {
//            base.OnBehaviorInitialize();

//            var networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
//            networkMessageHandlerRegisterer.RegisterBaseHandler<EnableVoiceChat>(new GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>(this.HandleEnableVoiceChat));
//        }

//        public override void OnRemoveBehavior()
//        {
//            base.OnRemoveBehavior();
//            var networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
//            networkMessageHandlerRegisterer.RegisterBaseHandler<EnableVoiceChat>(new GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>(this.HandleEnableVoiceChat));
//        }        

//        public override void OnMissionScreenInitialize()
//        {
//            base.OnMissionScreenInitialize();
//        }

//        private void HandleEnableVoiceChat(GameNetworkMessage networkMessage)
//        {
//            var message = networkMessage as EnableVoiceChat;
//            if (message != null)
//            {
//                VoiceChatEnabled = message.Enabled;
//                if (!message.Enabled)
//                {
//                    return;
//                }

//                this._proximityChatComponent = base.Mission.GetMissionBehavior<ProximityChatComponent>();
//                if (this._proximityChatComponent != null)
//                {
//                    this.IsActive = true;
//                    this._proximityChatComponent.OnVoicePlayMessage += HandleVoicePlayMessage;
//                    this._proximityChatComponent.OnVoicePlayerJoined += HandleVoicePlayerJoined;
//                    this._proximityChatComponent.OnVoicePlayerLeaved += HandleVoicePlayerLeaved;
//                    this._proximityChatComponent.OnOptionsClicked += HandleOptionsClicked;
//                }
//                try
//                {
//                    InitializeVoiceChatComponents();
//                }
//                catch (Exception e)
//                {
//                    this.IsActive = false;
//                    this.VoiceChatEnabled = false;
//                }

//                int waveInDevices = WaveIn.DeviceCount;
//                List<WaveInCapabilities> devices = new List<WaveInCapabilities>();
//                for (int waveInDevice = -1; waveInDevice < waveInDevices; waveInDevice++)
//                {
//                    try
//                    {
//                        WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(waveInDevice);
//                        devices.Add(deviceInfo);
//                    }
//                    catch (Exception e)
//                    {
//                        // 
//                    }
//                }

//                List<WaveOutCapabilities> outDevices = new List<WaveOutCapabilities>();
//                int waveOutDevices = WaveOut.DeviceCount;
//                for (int waveOutDevice = -1; waveOutDevice < waveOutDevices; waveOutDevice++)
//                {
//                    try
//                    {
//                        outDevices.Add(WaveOut.GetCapabilities(waveOutDevice));
//                    }
//                    catch (Exception e)
//                    {
//                        // 
//                    }
//                }

//                this._dataSource = new PEVoiceChatOptionsVM(devices, outDevices, true, this.CloseOptions, this.ApplyOptions, this.StartTest, this.StopTest);
//            }
//        }

//        private void CloseOptions()
//        {
//            if (this._gauntletLayer == null || this.MissionScreen == null) return;
//            this._gauntletLayer.InputRestrictions.ResetInputRestrictions();
//            base.MissionScreen.RemoveLayer(this._gauntletLayer);
//            this._gauntletLayer = null;
//        }
//        private void ApplyOptions(int inputDevice, int inputGain, int outputDevice, int outputGain, bool vcEnabled)
//        {
//            outputStream.DeviceNumber = outputDevice;
//            sourceStream.DeviceNumber = inputDevice;

//            outputVolume.Volume = (outputGain / 100f);
//            this.inputGain = (inputGain / 100f);
//            this.VoiceChatEnabled = vcEnabled;

//            try
//            {
//                if (this.IsActive == false)
//                {
//                    outputStream.Play();
//                    this.IsActive = true;
//                    this.VoiceChatEnabled = true;
//                }
//            }
//            catch (Exception e)
//            {
//                this.IsActive = false;
//                this.VoiceChatEnabled = false;
//            }

//            // this.CloseOptions();
//        }

//        private void StartTest(int device, int gain)
//        {
//            if (this.IsActive == false || this.selfBufferedWaveProvider == null || this.sourceStream == null || this.selfOutputStream == null)
//            {
//                return;
//            }
//            this.SelfRecording = true;
//            this.selfBufferedWaveProvider.ClearBuffer();
//            this.sourceStream.DeviceNumber = device;
//            this.inputGain = (gain / 100f);
//            this.selfOutputStream.Play();
//            this.sourceStream.StartRecording();
//        }
//        private void StopTest()
//        {
//            if (this.IsActive == false)
//            {
//                return;
//            }
//            this.SelfRecording = false;
//            this.selfBufferedWaveProvider.ClearBuffer();
//            this.selfOutputStream.Pause();
//            this.sourceStream.StopRecording();
//        }

//        private void HandleOptionsClicked()
//        {
//            this.OpenOptionsMenu();
//        }
//        private void OpenOptionsMenu()
//        {
//            this._gauntletLayer = new GauntletLayer("PEProximityChatView", 2);
//            this._gauntletLayer.LoadMovie("PEVoiceChatOptions", this._dataSource);
//            this._gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.Mouse);
//            base.MissionScreen.AddLayer(this._gauntletLayer);
//        }
//        public override void OnMissionScreenFinalize()
//        {
//            base.OnMissionScreenFinalize();
//        }

//        private void HandleVoicePlayerLeaved(NetworkCommunicator player)
//        {
//            if (this._playerVoiceData.ContainsKey(player))
//            {
//                this._playerVoiceData.Remove(player);
//            }
//        }

//        private void HandleVoicePlayerJoined(NetworkCommunicator player)
//        {
//            _playerVoiceData[player] = new PlayerVoiceData(player);
//        }

//        private void OnVoiceRecord(object sender, WaveInEventArgs e)
//        {
//            if (this.IsActive == false || this.VoiceChatEnabled == false) return;
//            if (this.IsRecording)
//            {
//                short[] pcmBuffer = new short[48 * bufferMs];
//                Buffer.BlockCopy(e.Buffer, 0, pcmBuffer, 0, e.BytesRecorded);
//                for (int i = 0; i < 48 * bufferMs; i++)
//                {
//                    pcmBuffer[i] = (short)((float)pcmBuffer[i] * inputGain);
//                }

//                int x = 0;
//                int size = 2880;
//                short[][] encodeChunks = pcmBuffer.GroupBy(s => x++ / size).Select(s => s.ToArray()).ToArray();

//                byte[][] byteBatch = new byte[encodeChunks.Length][];
//                int[] lengths = new int[encodeChunks.Length];
//                for (int i = 0; i < encodeChunks.Length; i++)
//                {
//                    byteBatch[i] = new byte[1440];
//                    int length = opusEncoder.Encode(encodeChunks[i], 0, 2880, byteBatch[i], 0, byteBatch[i].Length);
//                    lengths[i] = length;
//                }
//                this._proximityChatComponent.SendEncodedVoiceToServer(byteBatch, lengths);
//            }
//            else if (this.SelfRecording)
//            {
//                short[] pcmBuffer = new short[48 * bufferMs];
//                Buffer.BlockCopy(e.Buffer, 0, pcmBuffer, 0, e.BytesRecorded);
//                for (int i = 0; i < 48 * bufferMs; i++)
//                {
//                    pcmBuffer[i] = (short)((float)pcmBuffer[i] * inputGain);
//                }
//                byte[] manipulated = new byte[e.BytesRecorded];
//                Buffer.BlockCopy(pcmBuffer, 0, manipulated, 0, e.BytesRecorded);
//                selfBufferedWaveProvider.AddSamples(manipulated, 0, e.BytesRecorded);
//            }
//        }

//        public void HandleVoicePlayMessage(SendBatchVoiceToPlay message)
//        {
//            if (this.VoiceChatEnabled == false || this.IsActive == false || message.Peer == null) return;
//            if (this._playerVoiceData.ContainsKey(message.Peer))
//            {
//                int srcOffset = 0;
//                for (int i = 0; i < message.BufferLens.Length; i++)
//                {
//                    short[] outPcm = new short[5000];
//                    byte[] buffer = new byte[message.BufferLens[i]];
//                    Buffer.BlockCopy(message.PackedBuffer, srcOffset, buffer, 0, message.BufferLens[i]);
//                    srcOffset += message.BufferLens[i];
//                    int length = this._playerVoiceData[message.Peer].OpusDecoder.Decode(buffer, 0, message.BufferLens[i], outPcm, 0, outPcm.Length);
//                    byte[] outputByte = new byte[length * 2];
//                    Buffer.BlockCopy(outPcm, 0, outputByte, 0, outputByte.Length);
//                    this._playerVoiceData[message.Peer].AddSample(outputByte, 0, outputByte.Length);
//                    if (this._tempHaveData.Contains(message.Peer) == false)
//                    {
//                        this._tempHaveData.Add(message.Peer);
//                    }
//                    if (this.OnPeerVoiceStatusUpdated != null) this.OnPeerVoiceStatusUpdated(this._playerVoiceData[message.Peer], message.Peer);
//                }
//            }
//        }

//        public override void OnPreMissionTick(float dt)
//        {
//            base.OnPreMissionTick(dt);
//            if (bufferedWaveProvider == null) return;
//            if (GameNetwork.MyPeer == null || GameNetwork.MyPeer.ControlledAgent == null) return;
//            if (this.IsActive == false || this.VoiceChatEnabled == false) return;

//            Agent controlledAgent = GameNetwork.MyPeer.ControlledAgent;
//            if (controlledAgent == null) return;
//            int pcmSize = MathF.Ceiling(dt * 1000 * 48);

//            bool hasDataToPlay = false;
//            short[] mixedData = new short[pcmSize];
//            int count = 0;
//            foreach (NetworkCommunicator peer in this._tempHaveData.ToList())
//            {
//                if (this._playerVoiceData.ContainsKey(peer) == false)
//                {
//                    this._tempHaveData.Remove(peer);
//                    continue;
//                }
//                PlayerVoiceData voiceData = this._playerVoiceData[peer];
//                if (voiceData.InputWaveProvider.BufferedDuration.TotalMilliseconds > 0)
//                {
//                    hasDataToPlay = true;
//                    voiceData.HasVoiceData = true;
//                    Agent speakerAgent = peer.ControlledAgent;
//                    float distance = speakerAgent == null ? 31f : controlledAgent.Position.Distance(speakerAgent.Position);
//                    float distanceVolumeMultiplier = 1 - (distance / 31f);

//                    voiceData.VolumeSampleProvider.Volume = distanceVolumeMultiplier < 0 ? 0 : distanceVolumeMultiplier;
//                    if (peer.GetComponent<MissionPeer>().IsMutedFromGameOrPlatform)
//                    {
//                        voiceData.VolumeSampleProvider.Volume = 0;
//                    }

//                    byte[] output = new byte[pcmSize * 2];
//                    output = voiceData.ReadSample(pcmSize);
//                    short[] outputPcm = new short[pcmSize];
//                    Buffer.BlockCopy(output, 0, outputPcm, 0, output.Length);


//                    // Mix
//                    for (int i = 0; i < pcmSize; i++)
//                    {
//                        int total = (int)mixedData[i] + (int)outputPcm[i];
//                        if (total > Int16.MaxValue) total = Int16.MaxValue;
//                        if (total < Int16.MinValue) total = Int16.MinValue;
//                        mixedData[i] = (short)total;
//                    }
//                    count++;
//                    if (this.OnPeerVoiceStatusUpdated != null) this.OnPeerVoiceStatusUpdated(voiceData, peer);

//                }
//                if (voiceData.InputWaveProvider.BufferedDuration.TotalMilliseconds == 0) this._tempHaveData.Remove(peer);
//            }


//            if (hasDataToPlay)
//            {
//                for (int i = 0; i < pcmSize; i++)
//                {
//                    mixedData[i] = (short)(mixedData[i] / count);
//                }
//                byte[] readedByte = new byte[pcmSize * 2];
//                Buffer.BlockCopy(mixedData, 0, readedByte, 0, readedByte.Length);
//                try
//                {
//                    bufferedWaveProvider.AddSamples(readedByte, 0, pcmSize * 2);
//                }
//                catch (InvalidOperationException e)
//                {
//                    bufferedWaveProvider.ClearBuffer();
//                }
//            }
//        }

//        public override void OnMissionScreenTick(float dt)
//        {
//            base.OnMissionScreenTick(dt);
//            if (this.IsActive == false || this.VoiceChatEnabled == false) return;
//            try
//            {
//                if (Input.IsKeyPressed(TaleWorlds.InputSystem.InputKey.V) && !this.IsRecording)
//                {
//                    this.IsRecording = true;
//                    this.sourceStream.StartRecording();
//                }
//                if (Input.IsKeyReleased(TaleWorlds.InputSystem.InputKey.V) && this.IsRecording)
//                {
//                    this.IsRecording = false;
//                    this.sourceStream.StopRecording();
//                }
//            }
//            catch (MmException e)
//            {
//                InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("PlayerVoiceDataError", null).ToString(), Color.ConvertStringToColor("#FF0000FF")));
//            }
//            catch (InvalidOperationException e)
//            {
//                // InformationManager.DisplayMessage(new InformationMessage("Invalid microphone please change it from your options menu.", Color.ConvertStringToColor("#FF0000FF")));
//            }
//        }


//    }
//}
