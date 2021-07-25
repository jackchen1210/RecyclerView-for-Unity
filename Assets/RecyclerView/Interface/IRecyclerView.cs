using UnityEngine;

namespace RecyclerView{


    interface IRecyclerView
    {
        void ScrollTo(Vector2 pos);
        void ScrollTo(int position);
        void SmothScrollTo(int position);

    }
}