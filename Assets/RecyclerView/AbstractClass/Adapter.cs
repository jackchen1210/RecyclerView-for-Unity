using UnityEngine;

namespace RecyclerView{

    public abstract class Adapter<T> : RecyclerView<T>, IDataObservable
        where T : ViewHolder
    {
        /// <summary>
        /// 
        /// </summary>
        public void NotifyDatasetChanged()
        {
            OnDataChange();
        }
        /// <summary>
        /// Scroll to a certain position from [0, 
        /// </summary>
        /// <param name="pos"></param>
        public void ScrollBy(Vector2 pos)
        {
            layoutManager.ScrollTo(pos);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        public void ScrollTo(int position)
        {
            layoutManager.ScrollTo(position);

        }
        /// <summary>
        /// /
        /// </summary>
        /// <param name="position"></param>
        public void SmothScrollTo(int position)
        {
            layoutManager.SmothScrollTo(position);
        }
    }

}