/*
    MIT License

    Copyright (c) 2019 Framg

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE. 
*/



using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if (UNITY_EDITOR) 
using UnityEditor;
using System.IO;
#endif
namespace RecyclerView
{
    /// <summary>
    /// Recycler view for Unity.
    /// List of elements, it use pooling to display items in the screen. So it's going to keep a small list instead of the full elements list.
    /// </summary>
    /// <typeparam name="T">T must be an extension of ViewHolder from RecyclerView.</typeparam>
    public abstract class RecyclerView<T> : MonoBehaviour,IAdapter<T>
        where T : ViewHolder
    {

        #if (UNITY_EDITOR)
        [Range(0, 1f)]
        [ReadOnlyWhenPlaying]
        #endif
        public float decelerationRate = 0.5f;

        #if (UNITY_EDITOR)
        [ReadOnlyWhenPlaying]
        [Header("List orientation")]
        #endif
        public Orientation orientation;

        #if (UNITY_EDITOR)
        [ReadOnlyWhenPlaying]
        [Header("Margin between rows")]
        #endif
        public Vector2 Spacing;

        #if (UNITY_EDITOR)
        [ReadOnlyWhenPlaying]
        [Header("Set true to make the list reverse")]
        #endif
        public bool IsReverse;

        #if (UNITY_EDITOR)
        [Space]
        [ReadOnlyWhenPlaying]
        [Header("Pool size and cache size (do not modify if you are not sure)")]
        #endif
        public int PoolSize = 3;

        #if (UNITY_EDITOR)
        [ReadOnlyWhenPlaying]
        #endif
        public int CacheSize = 3;

        private Pool pool;
        internal readonly List<IViewHolderInfo> AttachedScrap = new List<IViewHolderInfo>();
        private readonly List<IViewHolderInfo> Cache = new List<IViewHolderInfo>();

        public abstract GameObject OnCreateViewHolder();
        public abstract void OnBindViewHolder(T holder, int i);   
        public abstract int GetItemCount();

        protected LayoutManager<T> layoutManager;


        private void Awake()
        {
            layoutManager = new LayoutManager<T>(this, orientation);

            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            layoutManager.Create();

            OnDataChange();
        }

        private void AddToAttachedScrap(IViewHolderInfo vh, bool up)
        {
            layoutManager.AttachToGrid(vh, up);         
            vh.ItemView.SetActive(true);
            AttachedScrap.Add(vh);
        }  


        private IViewHolderInfo TryGetViewHolderForPosition(int position)
        {
            if (position >= 0 && position < GetItemCount())
            {
                for(int i=0; i<AttachedScrap.Count; i++)
                {
                    if(AttachedScrap[i].CurrentIndex == position)
                    {
                        IViewHolderInfo v = AttachedScrap[i];
                        AttachedScrap.RemoveAt(i);
                        return v;
                    }
                }

                for(int i=0; i<Cache.Count; i++)
                {
                    if(Cache[i].CurrentIndex == position)
                    {
                        IViewHolderInfo v = Cache[i];
                        Cache.RemoveAt(i);
                        return v;
                    }
                }

                IViewHolderInfo vhrecycled;
                vhrecycled = pool.GetFromPool();
                if (vhrecycled != null)
                {
                    vhrecycled.Status = Status.SCRAP;
                    vhrecycled.LastIndex = vhrecycled.CurrentIndex;
                    vhrecycled.CurrentIndex = position;
                    layoutManager.AttachToGrid(vhrecycled, true);
                    OnBindViewHolder((T)Convert.ChangeType(vhrecycled, typeof(T)), vhrecycled.CurrentIndex);
                    return vhrecycled;
                }


                IViewHolderInfo vh = (ViewHolder)Activator.CreateInstance(typeof(T), new object[] { OnCreateViewHolder() });
                vh.CurrentIndex = position;
                vh.LastIndex = position;
                vh.Status = Status.SCRAP;
                layoutManager.AttachToGrid(vh, true);
                OnBindViewHolder((T)Convert.ChangeType(vh, typeof(T)), vh.CurrentIndex);
                return vh;


            }
            else
            {
                return null;
            }
        }

        private void ThrowAttachedScrapToCache()
        {
            foreach (IViewHolderInfo vh in AttachedScrap)
            {
                ThrowToCache(vh);
            }
        }

        internal void UpdateScrap()
        {
            int firstPosition = layoutManager.GetFirstPosition();
            List<IViewHolderInfo> TmpScrap = new List<IViewHolderInfo>();
            
            for(int i=firstPosition - 1; i< firstPosition + layoutManager.GetScreenListSize()+1; i++)
            {
                IViewHolderInfo vh = TryGetViewHolderForPosition(i);
                if(vh != null)
                {
                    TmpScrap.Add(vh);
                }
            }

            ThrowAttachedScrapToCache();
            AttachedScrap.Clear();
            AttachedScrap.AddRange(TmpScrap);
        }


        public override string ToString()
        {
            string str = "";
            str += "Attached: {";
            foreach (IViewHolderInfo vh in AttachedScrap)
            {
                str += vh.CurrentIndex + ",";
            }
            str += "} Cache: {";
            foreach (IViewHolderInfo vh in Cache)
            {
                str += vh.CurrentIndex + ",";
            }
            str += "} Pool: {";
            foreach (IViewHolderInfo vh in pool.Scrap)
            {
                str += vh.CurrentIndex + ",";
            }
            str += "}";
            return str;
        }

        private void ThrowToPool(IViewHolderInfo vh)
        {
 
            vh.Status = Status.RECYCLED;
            vh.ItemView.SetActive(false);
            IViewHolderInfo recycled = pool.Throw(vh);
            if(recycled != null)
            {
                recycled.Destroy();
            }
            
        }



        private void ThrowToCache(IViewHolderInfo viewHolder)
        {
            viewHolder.Status = Status.CACHE;
            Cache.Add(viewHolder);
            if (Cache.Count > CacheSize)
            {
                ThrowToPool(Cache[0]);
                Cache.RemoveAt(0);
            }
        }


        private void Clear()
        {
            layoutManager.Clear();

            AttachedScrap.Clear();
            pool = null;
            
            Cache.Clear();

        }

        internal void OnDataChange(int pos = 0)
        {
            layoutManager.IsCreating = true;

            if (pos < 0 || pos > GetItemCount())
            {
                return;
            }
            
            Clear();

            pool = new Pool(PoolSize, CacheSize);

            if (GetItemCount() > 0)
            {
                IViewHolderInfo vh = (T)Activator.CreateInstance(typeof(T), new object[] { OnCreateViewHolder() });
                vh.CurrentIndex = pos;
                vh.LastIndex = pos;
                vh.Status = Status.SCRAP;
                AddToAttachedScrap(vh, true);
                layoutManager.SetPositionViewHolder(vh);
                OnBindViewHolder((T)Convert.ChangeType(vh, typeof(T)), pos);

               
                    
                layoutManager.OnDataChange(vh.ItemView, pos);

                int ATTACHED_SCRAP_SIZE = layoutManager.GetScreenListSize() + 1;

                for (int i = pos + 1; i < ATTACHED_SCRAP_SIZE + pos; i++)
                {
                    if (i < GetItemCount())
                    {
                        IViewHolderInfo vh2 = (T)Activator.CreateInstance(typeof(T), new object[] { OnCreateViewHolder() });
                        vh2.CurrentIndex = i;
                        vh2.LastIndex = i;
                        vh2.Status = Status.SCRAP;
                        AddToAttachedScrap(vh2, true);
                        layoutManager.SetPositionViewHolder(vh2);
                        OnBindViewHolder((T)Convert.ChangeType(vh2, typeof(T)), i);
                    }
                }            
                layoutManager.ClampList();
            }

            layoutManager.IsCreating = false;
        }



    }
    

}
