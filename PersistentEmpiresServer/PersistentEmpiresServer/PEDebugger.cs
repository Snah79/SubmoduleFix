using System.Runtime.CompilerServices;
using TaleWorlds.Library;

namespace PersistentEmpiresServer
{
    internal class PEDebugger : IDebugManager
    {
        public void AbortGame()
        {
        }

        public void Assert(bool condition, string message, [CallerFilePath] string callerFile = "", [CallerMemberName] string callerMethod = "", [CallerLineNumber] int callerLine = 0)
        {
        }

        public void DisplayDebugMessage(string message)
        {
        }

        public void DoDelayedexit(int returnCode)
        {
        }

        public Vec3 GetDebugVector()
        {
            return new Vec3();
        }

        public void Print(string message, int logLevel = 0, Debug.DebugColor color = Debug.DebugColor.White, ulong debugFilter = 17592186044416)
        {
        }

        public void PrintError(string error, string stackTrace, ulong debugFilter = 17592186044416)
        {
        }

        public void PrintWarning(string warning, ulong debugFilter = 17592186044416)
        {
        }

        public void RenderDebugFrame(MatrixFrame frame, float lineLength, float time = 0)
        {
        }

        public void RenderDebugLine(Vec3 position, Vec3 direction, uint color = uint.MaxValue, bool depthCheck = false, float time = 0)
        {
        }

        public void RenderDebugRectWithColor(float left, float bottom, float right, float top, uint color = uint.MaxValue)
        {
        }

        public void RenderDebugSphere(Vec3 position, float radius, uint color = uint.MaxValue, bool depthCheck = false, float time = 0)
        {
        }

        public void RenderDebugText(float screenX, float screenY, string text, uint color = uint.MaxValue, float time = 0)
        {
        }

        public void RenderDebugText3D(Vec3 position, string text, uint color = uint.MaxValue, int screenPosOffsetX = 0, int screenPosOffsetY = 0, float time = 0)
        {
        }

        public void ReportMemoryBookmark(string message)
        {
        }

        public void SetCrashReportCustomStack(string customStack)
        {
        }

        public void SetCrashReportCustomString(string customString)
        {
        }

        public void SetDebugVector(Vec3 value)
        {
        }

        public void SetTestModeEnabled(bool testModeEnabled)
        {
        }

        public void ShowError(string message)
        {
        }

        public void ShowMessageBox(string lpText, string lpCaption, uint uType)
        {
        }

        public void ShowWarning(string message)
        {
        }

        public void SilentAssert(bool condition, string message = "", bool getDump = false, [CallerFilePath] string callerFile = "", [CallerMemberName] string callerMethod = "", [CallerLineNumber] int callerLine = 0)
        {
        }

        public void WatchVariable(string name, object value)
        {
        }

        public void WriteDebugLineOnScreen(string message)
        {
        }
    }
}