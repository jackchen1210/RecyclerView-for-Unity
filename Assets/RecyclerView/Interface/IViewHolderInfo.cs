
using UnityEngine;

namespace RecyclerView
{
    internal interface IViewHolderInfo

    {
        int LastIndex { get; set; }
        int CurrentIndex { get; set; }
        GameObject ItemView { get; set; }
        RectTransform RectTransform { get; set; }
        Status Status { get; set; }
        void Destroy();
        bool IsHidden();
    }
}
