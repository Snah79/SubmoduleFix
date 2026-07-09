using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Core.ImageIdentifiers;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpires.Views.ViewsVM.AdminPanel
{
    public class PEAdminItemPanelVM : ViewModel
    {
        private string _itemId;
        private Action _onCancel;
        private Action<string, int> _onApply;
        private int _count = 1;

        public PEAdminItemPanelVM(Action<string, int> _onApply, Action _onCancel)
        {
            this._onApply = _onApply;
            this._onCancel = _onCancel;
        }

        string tmp = "";
        List<ItemObject> tmpFoundItems;
        private bool Find(ItemObject item)
        {
            return item.StringId.ToLower().Contains(tmp) && !tmpFoundItems.Contains(item);
        }

        private void TryToFindItem()
        {
            try
            {
                var items = MBObjectManager.Instance.GetObjects<ItemObject>(Find);

                tmpFoundItems = new List<ItemObject>();
                tmpFoundItems.AddRange(items.Select(x => x));
                //if (item != null)
                //{
                //    tmpFoundItems.Add(item.StringId);
                //    TryToFindItem();
                //}
                //else
                //{
                if (tmpFoundItems.Any())
                {
                    if (tmpFoundItems.Count() == 1)
                    {
                        ItemId = tmpFoundItems[0].StringId;
                        RefreshValues();
                    }
                    else
                    {
                        MBInformationManager.ShowMultiSelectionInquiry(
                    new MultiSelectionInquiryData(GameTexts.FindText("PEAdminItemPanelInqCaption", null).ToString()
                    , GameTexts.FindText("PEAdminItemPanelInqText", null).ToString()
                    , tmpFoundItems.OrderBy(x => x).Select(x => new InquiryElement(x, $"{x.StringId}", new ItemImageIdentifier(x, ""))).ToList()
                    , true
                    , 1
                    , 1
                    , GameTexts.FindText("PE_InquiryData_Select", null).ToString()
                    , GameTexts.FindText("PE_InquiryData_Cancel", null).ToString()
                    , DoSelectItem
                    , DoCancel));
                    }
                }
                //}
            }
            catch (Exception ex)
            {

            }
        }

        private void DoCancel(List<InquiryElement> list)
        {
            tmpFoundItems = null;
        }

        private void DoSelectItem(List<InquiryElement> list)
        {
            var itemId = list.FirstOrDefault().Identifier as string;

            ItemId = itemId;
            RefreshValues();
        }        

        [DataSourceProperty]
        public string ItemId
        {
            get => this._itemId;
            set
            {
                if (value != this._itemId)
                {
                    this._itemId = value;
                    base.OnPropertyChangedWithValue(value, "ItemId");

                    if (_itemId.EndsWith("*"))
                    {
                        tmp = _itemId?.TrimEnd('*')?.ToLower();
                        if (_itemId.Length > 3)
                        {
                            tmpFoundItems = new List<ItemObject>();

                            TryToFindItem();
                        }
                    }
                }
            }
        }

        [DataSourceProperty]
        public int Count
        {
            get => this._count;
            set
            {
                if (value != this._count)
                {
                    this._count = value;
                    base.OnPropertyChangedWithValue(value, "Count");
                }
            }
        }

        public void ExecuteApply()
        {
            this._onApply(this.ItemId, this.Count);
        }
        public void ExecuteCancel()
        {
            this._onCancel();
        }
    }
}
