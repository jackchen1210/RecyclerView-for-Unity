using UnityEngine;

namespace RecyclerView{


    interface IAdapter<T>
    {
        GameObject OnCreateViewHolder();
        void OnBindViewHolder(T holder, int i);
        int GetItemCount();
    }
}