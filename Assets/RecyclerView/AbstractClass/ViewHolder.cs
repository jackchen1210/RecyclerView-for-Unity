using UnityEngine;

namespace RecyclerView
{

    public abstract class ViewHolder : IViewHolderInfo
    {
        GameObject itemView;
        RectTransform rectTransform;
        int last_index, current_index;
        Status status;
        long timeStamp;

        int IViewHolderInfo.LastIndex { get => last_index; set => last_index = value; }
        int IViewHolderInfo.CurrentIndex { get => current_index; set => current_index = value; }
        GameObject IViewHolderInfo.ItemView { get => itemView; set => itemView = value; }
        RectTransform IViewHolderInfo.RectTransform { get => rectTransform; set => rectTransform = value; }
        public Status Status { get => status; set => status = value; }

        public ViewHolder(GameObject itemView)
        {
            this.itemView = itemView;
            this.rectTransform = itemView.GetComponent<RectTransform>();

        }

        public int GetAdapterPosition()
        {
            return current_index;
        }

        private void Destroy()
        {
            GameObject.Destroy(itemView);
        }

        private bool IsHidden()
        {
            return !IsVisibleFrom(itemView.GetComponent<RectTransform>(), Camera.main);
        }

        private static bool IsVisibleFrom(RectTransform rectTransform, Camera camera)
        {
            return CountCornersVisibleFrom(rectTransform, camera) > 0;
        }
        private static int CountCornersVisibleFrom(RectTransform rectTransform, Camera camera)
        {
            Rect screenBounds = new Rect(0f, 0f, Screen.width, Screen.height);
            Vector3[] objectCorners = new Vector3[4];
            rectTransform.GetWorldCorners(objectCorners);

            int visibleCorners = 0;
            for (var i = 0; i < objectCorners.Length; i++)
            {

                if (screenBounds.Contains(objectCorners[i]))
                {
                    visibleCorners++;
                }
            }
            return visibleCorners;
        }

        private int CompareTo(ViewHolder vh)
        {
            if (vh.current_index > this.current_index)
            {
                return -1;
            }
            else if (vh.current_index > this.current_index)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        void IViewHolderInfo.Destroy()
        {
            Destroy();
        }

        bool IViewHolderInfo.IsHidden()
        {
            return IsHidden();
        }
    }

}