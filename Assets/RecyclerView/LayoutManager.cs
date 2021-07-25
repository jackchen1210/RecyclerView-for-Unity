using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RecyclerView{


    public class LayoutManager<T>:MonoBehaviour where T :ViewHolder
    {
        private Orientation orientation;
        private Vector2 RowDimension;
        private Vector2 RowScale;
        private ScrollRect ScrollRect;
        private RectTransform SelfRectTransform { get; set; }
        private RectTransform GridRectTransform { get; set; }
        private GameObject Grid;
        public float LIMIT_BOTTOM = 0;
        public bool IsCreating = false;
        private bool isDraging, isClickDown;

        private RecyclerView<T> recyclerView;

        public LayoutManager(RecyclerView<T> recyclerView, Orientation orientation)
        {
            this.recyclerView = recyclerView;
            this.orientation = orientation;
        }

        public int GetFirstPosition()
        {
            if (IsVerticalOrientation())
            {
                if (recyclerView.IsReverse)
                {
                    return Mathf.Abs(Mathf.RoundToInt(Mathf.Clamp(Grid.transform.GetComponent<RectTransform>().offsetMin.y, -LIMIT_BOTTOM, 0) / (GetRowSize().y)));

                }
                else
                {
                    return Mathf.RoundToInt(Mathf.Clamp(Grid.transform.GetComponent<RectTransform>().offsetMin.y, 0, LIMIT_BOTTOM) / (GetRowSize().y));

                }
            }
            else
            {


                if (recyclerView.IsReverse)
                {
                    return Mathf.RoundToInt(Mathf.Clamp(Grid.transform.GetComponent<RectTransform>().offsetMin.x, 0, LIMIT_BOTTOM) / (GetRowSize().x));

                }
                else
                {
                    return Mathf.Abs(Mathf.RoundToInt(Mathf.Clamp(Grid.transform.GetComponent<RectTransform>().offsetMin.x, -LIMIT_BOTTOM, 0) / (GetRowSize().x)));

                }
            }



        }

        public int GetScreenListSize()
        {
            if (!IsVerticalOrientation())
            {
                return Mathf.FloorToInt(Screen.width / GetRowSize().x);
            }
            else
            {
                return Mathf.FloorToInt(Screen.height / GetRowSize().y);
            }

        }

        public void Create()
        {

            SelfRectTransform = recyclerView.GetComponent<RectTransform>();
            Grid = new GameObject();
            Grid.name = "Grid";
            GridRectTransform = Grid.AddComponent<RectTransform>();
            GridRectTransform.sizeDelta = Vector2.zero;

            if (IsVerticalOrientation())
            {
                if (recyclerView.IsReverse)
                {
                    GridRectTransform.anchorMax = new Vector2(0.5f, 0f);
                    GridRectTransform.anchorMin = new Vector2(0.5f, 0f);
                    GridRectTransform.pivot = new Vector2(0.5f, 0f);
                }
                else
                {
                    GridRectTransform.anchorMax = new Vector2(0.5f, 1f);
                    GridRectTransform.anchorMin = new Vector2(0.5f, 1f);
                    GridRectTransform.pivot = new Vector2(0.5f, 1f);
                }

            }
            else
            {
                if (recyclerView.IsReverse)
                {
                    GridRectTransform.anchorMax = new Vector2(1f, 0.5f);
                    GridRectTransform.anchorMin = new Vector2(1f, 0.5f);
                    GridRectTransform.pivot = new Vector2(1f, 0.5f);
                }
                else
                {
                    GridRectTransform.anchorMax = new Vector2(0f, 0.5f);
                    GridRectTransform.anchorMin = new Vector2(0f, 0.5f);
                    GridRectTransform.pivot = new Vector2(0f, 0.5f);
                }
            }

            Grid.transform.SetParent(recyclerView.transform);
            GridRectTransform.anchoredPosition = Vector3.zero;


            ScrollRect = recyclerView.GetComponent<ScrollRect>();
            if (ScrollRect == null)
            {
                ScrollRect = recyclerView.gameObject.AddComponent<ScrollRect>();
            }
            ScrollRect.content = GridRectTransform;
            ScrollRect.onValueChanged.AddListener(delegate { OnScroll(); });
            ScrollRect.viewport = SelfRectTransform;
            ScrollRect.content = GridRectTransform;
            ScrollRect.movementType = ScrollRect.MovementType.Unrestricted;
            ScrollRect.inertia = true;
            ScrollRect.decelerationRate = recyclerView.decelerationRate;
            ScrollRect.scrollSensitivity = 10f;
            ScrollRect.vertical = IsVerticalOrientation();
            ScrollRect.horizontal = !IsVerticalOrientation();

            if (recyclerView.GetComponent<Image>() == null)
            {
                Image image = recyclerView.gameObject.AddComponent<Image>();
                image.color = new Color(0, 0, 0, 0.01f);
            }
            if (recyclerView.GetComponent<Mask>() == null)
            {
                recyclerView.gameObject.AddComponent<Mask>();
            }

            if (recyclerView.gameObject.GetComponent<EventTrigger>() == null)
            {
                EventTrigger eventTrigger = recyclerView.gameObject.AddComponent<EventTrigger>();
                EventTrigger.Entry pointup = new EventTrigger.Entry();
                pointup.eventID = EventTriggerType.PointerUp;
                pointup.callback.AddListener((data) => { OnClickUp(); });
                eventTrigger.triggers.Add(pointup);

                EventTrigger.Entry pointdown = new EventTrigger.Entry();
                pointdown.eventID = EventTriggerType.PointerDown;
                pointdown.callback.AddListener((data) => { OnClickDown(); });
                eventTrigger.triggers.Add(pointdown);

                EventTrigger.Entry drag = new EventTrigger.Entry();
                drag.eventID = EventTriggerType.Drag;
                drag.callback.AddListener((data) => { OnDrag(); });
                eventTrigger.triggers.Add(drag);
            }
        }

        private void OnDrag()
        {
            isDraging = true;
        }

        private void OnClickDown()
        {
            isClickDown = true;
        }

        private void OnClickUp()
        {
            isDraging = false;
            isClickDown = false;
        }

        public Vector2 GetRowSize()
        {
            if (IsVerticalOrientation())
            {
                return new Vector2(0, ((RowDimension.y * RowScale.y) + recyclerView.Spacing.y));
            }
            else
            {
                return new Vector2(((RowDimension.x * RowScale.x) + recyclerView.Spacing.x), 0);
            }
        }


        internal void SetPositionViewHolder(IViewHolderInfo vh)
        {
            Vector2 size = GetRowSize();


            if (IsVerticalOrientation())
            {
                if (recyclerView.IsReverse)
                {
                    vh.RectTransform.localPosition = new Vector3(0, (vh.CurrentIndex * size.y), 0);
                }
                else
                {
                    vh.RectTransform.localPosition = new Vector3(0, (-vh.CurrentIndex * size.y), 0);
                }
            }
            else
            {
                if (recyclerView.IsReverse)
                {
                    vh.RectTransform.localPosition = new Vector3((-vh.CurrentIndex * size.x), 0, 0);
                }
                else
                {
                    vh.RectTransform.localPosition = new Vector3((vh.CurrentIndex * size.x), 0, 0);
                }
            }
        }



        private void Invalidate()
        {

            if (IsVerticalOrientation())
            {
                if (recyclerView.IsReverse)
                {
                    if (GridRectTransform.offsetMax.y < -LIMIT_BOTTOM)
                    {
                        recyclerView.OnDataChange(recyclerView.GetItemCount() - 1);
                    }
                    else
                    {
                        recyclerView.OnDataChange(0);
                    }
                }
                else
                {
                    if (GridRectTransform.offsetMax.y > LIMIT_BOTTOM)
                    {
                        recyclerView.OnDataChange(recyclerView.GetItemCount() - 1);
                    }
                    else
                    {
                        recyclerView.OnDataChange(0);
                    }
                }
            }
            else
            {
                if (recyclerView.IsReverse)
                {
                    if (GridRectTransform.offsetMax.x > LIMIT_BOTTOM)
                    {
                        recyclerView.OnDataChange(recyclerView.GetItemCount() - 1);
                    }
                    else
                    {
                        recyclerView.OnDataChange(0);
                    }
                }
                else
                {
                    if (GridRectTransform.offsetMax.x < -LIMIT_BOTTOM)
                    {
                        recyclerView.OnDataChange(recyclerView.GetItemCount() - 1);
                    }
                    else
                    {
                        recyclerView.OnDataChange(0);
                    }
                }
            }
        }

        private void OnScroll()
        {
            if (!IsCreating)
            {
                if (IsStateValid())
                {
                    recyclerView.UpdateScrap();
                    ClampList();
                }
                else
                {
                    Invalidate();
                }

            }
        }

        public void OnDataChange(GameObject initialVH, int pos = 0)
        {
            RowDimension = new Vector2(initialVH.GetComponent<RectTransform>().rect.width, initialVH.GetComponent<RectTransform>().rect.height);
            RowScale = initialVH.GetComponent<RectTransform>().localScale;
            Vector2 RowSize = GetRowSize();

            if (IsVerticalOrientation())
            {
                LIMIT_BOTTOM = ((recyclerView.GetItemCount() * RowSize.y) - SelfRectTransform.rect.height) - recyclerView.Spacing.y;

                if (recyclerView.IsReverse)
                {
                    GridRectTransform.offsetMax = new Vector2(GridRectTransform.offsetMax.x, -RowSize.y * pos);
                    GridRectTransform.sizeDelta = new Vector2(GridRectTransform.sizeDelta.x, 0);
                }
                else
                {
                    GridRectTransform.offsetMax = new Vector2(GridRectTransform.offsetMax.x, RowSize.y * pos);
                    GridRectTransform.sizeDelta = new Vector2(GridRectTransform.sizeDelta.x, 0);
                }

            }
            else
            {
                LIMIT_BOTTOM = ((recyclerView.GetItemCount() * RowSize.x) - SelfRectTransform.rect.width) - recyclerView.Spacing.x;


                if (recyclerView.IsReverse)
                {
                    GridRectTransform.offsetMax = new Vector2(RowSize.x * pos, GridRectTransform.offsetMax.y);
                    GridRectTransform.sizeDelta = new Vector2(0, GridRectTransform.sizeDelta.y);
                }
                else
                {
                    GridRectTransform.localPosition = new Vector2(-RowSize.x * pos, GridRectTransform.localPosition.y);
                    GridRectTransform.offsetMax = new Vector2(-RowSize.x * pos, GridRectTransform.offsetMax.y);
                    GridRectTransform.sizeDelta = new Vector2(0, GridRectTransform.sizeDelta.y);
                }

            }
        }

        private IEnumerator IScrollTo(Vector2 dir, float speed = 50)
        {
            ScrollRect.inertia = false;
            if (IsVerticalOrientation())
            {
                Vector2 v = new Vector2(0, dir.y * LIMIT_BOTTOM);
                bool goUp = GridRectTransform.offsetMax.y > v.y;
                float y = GridRectTransform.offsetMax.y;
                while (goUp ? GridRectTransform.offsetMax.y > v.y : GridRectTransform.offsetMax.y < v.y)
                {
                    y += goUp ? -speed : speed;

                    if (y > LIMIT_BOTTOM)
                    {
                        y = LIMIT_BOTTOM;
                        GridRectTransform.offsetMax = new Vector2(GridRectTransform.offsetMax.x, y);
                        GridRectTransform.sizeDelta = new Vector2(GridRectTransform.sizeDelta.x, 0);
                        OnScroll();
                        break;
                    }

                    GridRectTransform.offsetMax = new Vector2(GridRectTransform.offsetMax.x, y);
                    GridRectTransform.sizeDelta = new Vector2(GridRectTransform.sizeDelta.x, 0);
                    OnScroll();
                    yield return new WaitForEndOfFrame();


                    if (isClickDown)
                    {
                        break;
                    }
                }
            }
            else
            {
                Vector2 v = new Vector2(dir.x * LIMIT_BOTTOM, 0);
                bool goUp = GridRectTransform.offsetMax.x > v.x;
                float y = GridRectTransform.offsetMax.x;
                while (goUp ? GridRectTransform.offsetMax.x > v.x : GridRectTransform.offsetMax.x < v.x)
                {


                    y += goUp ? -speed : speed;

                    if (y > LIMIT_BOTTOM)
                    {
                        y = LIMIT_BOTTOM;
                        GridRectTransform.offsetMax = new Vector2(GridRectTransform.offsetMax.x, y);
                        GridRectTransform.sizeDelta = new Vector2(GridRectTransform.sizeDelta.x, 0);
                        OnScroll();
                        break;
                    }

                    GridRectTransform.offsetMax = new Vector2(GridRectTransform.offsetMax.x, y);
                    GridRectTransform.sizeDelta = new Vector2(GridRectTransform.sizeDelta.x, 0);
                    OnScroll();
                    yield return new WaitForEndOfFrame();

                    if (isClickDown)
                    {
                        break;
                    }

                }
            }
            ScrollRect.inertia = true;
        }

        public void ScrollTo(Vector2 pos)
        {
            recyclerView.StartCoroutine(IScrollTo(pos));
        }

        public void ScrollTo(int position)
        {
            recyclerView.StartCoroutine(INotifyDatasetChanged(position));

        }

        public void SmothScrollTo(int position)
        {
            if (IsVerticalOrientation())
            {
                recyclerView.StartCoroutine(IScrollTo(new Vector2(0, (GetRowSize().y * position) / LIMIT_BOTTOM)));
            }
            else
            {
                recyclerView.StartCoroutine(IScrollTo(new Vector2(((GetRowSize().x * position) / LIMIT_BOTTOM), 0)));
            }
        }


        private IEnumerator INotifyDatasetChanged(int pos = 0)
        {
            ScrollRect.inertia = false;
            recyclerView.OnDataChange(pos);
            yield return new WaitForEndOfFrame();
            OnScroll();
            ScrollRect.inertia = true;
        }




        internal void AttachToGrid(IViewHolderInfo vh, bool up)
        {
            vh.ItemView.transform.SetParent(Grid.transform);
            if (up)
            {
                vh.ItemView.transform.SetAsLastSibling();
            }
            else
            {
                vh.ItemView.transform.SetAsFirstSibling();
            }
            vh.ItemView.name = vh.CurrentIndex.ToString();
            vh.ItemView.SetActive(true);
            SetPivot(vh.RectTransform);
            SetPositionViewHolder(vh);
        }



        private bool IsStateValid()
        {
            if (recyclerView.GetItemCount() == 0)
            {
                return true;
            }
            foreach (IViewHolderInfo vh in recyclerView.AttachedScrap)
            {
                if (!vh.IsHidden())
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsVerticalOrientation()
        {
            return orientation == Orientation.VERTICAL;
        }

        public void Clear()
        {
            foreach (Transform row in Grid.transform)
            {
                Destroy(row.gameObject);
            }
        }

        private void SetPivot(RectTransform rect)
        {
            if (IsVerticalOrientation())
            {
                if (recyclerView.IsReverse)
                {
                    rect.pivot = new Vector2(0.5f, 0f);
                }
                else
                {
                    rect.pivot = new Vector2(0.5f, 1f);
                }
            }
            else
            {
                if (recyclerView.IsReverse)
                {
                    rect.pivot = new Vector2(1f, 0.5f);
                }
                else
                {
                    rect.pivot = new Vector2(0f, 0.5f);
                }
            }
        }


        public void ClampList()
        {
            if (IsVerticalOrientation())
            {
                if (recyclerView.IsReverse)
                {
                    if (GridRectTransform.offsetMax.y > 0)
                    {
                        GridRectTransform.localPosition = new Vector2(GridRectTransform.localPosition.x, 0);
                        GridRectTransform.offsetMax = new Vector2(GridRectTransform.offsetMax.x, 0);
                        GridRectTransform.sizeDelta = new Vector2(GridRectTransform.sizeDelta.x, 0);
                    }
                    else if (GridRectTransform.offsetMax.y < -LIMIT_BOTTOM)
                    {
                        GridRectTransform.localPosition = new Vector2(GridRectTransform.localPosition.x, -LIMIT_BOTTOM);
                        GridRectTransform.offsetMax = new Vector2(GridRectTransform.offsetMax.x, -LIMIT_BOTTOM);
                        GridRectTransform.sizeDelta = new Vector2(GridRectTransform.sizeDelta.x, 0);
                    }
                }
                else
                {
                    if (GridRectTransform.offsetMax.y < 0)
                    {
                        GridRectTransform.offsetMax = new Vector2(GridRectTransform.offsetMax.x, 0);
                        GridRectTransform.sizeDelta = new Vector2(GridRectTransform.sizeDelta.x, 0);
                    }
                    else if (GridRectTransform.offsetMax.y > LIMIT_BOTTOM)
                    {
                        GridRectTransform.offsetMax = new Vector2(GridRectTransform.offsetMax.x, LIMIT_BOTTOM);
                        GridRectTransform.sizeDelta = new Vector2(GridRectTransform.sizeDelta.x, 0);
                    }
                }

            }
            else
            {

                if (recyclerView.IsReverse)
                {
                    if (GridRectTransform.offsetMax.x < 0)
                    {
                        GridRectTransform.offsetMax = new Vector2(0, GridRectTransform.offsetMax.y);
                        GridRectTransform.sizeDelta = new Vector2(0, GridRectTransform.sizeDelta.y);
                    }
                    else if (GridRectTransform.offsetMax.x > LIMIT_BOTTOM)
                    {
                        GridRectTransform.offsetMax = new Vector2(LIMIT_BOTTOM, GridRectTransform.offsetMax.y);
                        GridRectTransform.sizeDelta = new Vector2(0, GridRectTransform.sizeDelta.y);
                    }
                }
                else
                {
                    if (GridRectTransform.offsetMax.x > 0)
                    {
                        GridRectTransform.localPosition = new Vector2(0, GridRectTransform.localPosition.y);
                        GridRectTransform.offsetMax = new Vector2(0, GridRectTransform.offsetMax.y);
                        GridRectTransform.sizeDelta = new Vector2(0, GridRectTransform.sizeDelta.y);
                    }
                    else if (GridRectTransform.offsetMax.x < -LIMIT_BOTTOM)
                    {
                        GridRectTransform.localPosition = new Vector2(-LIMIT_BOTTOM, GridRectTransform.localPosition.y);
                        GridRectTransform.offsetMax = new Vector2(-LIMIT_BOTTOM, GridRectTransform.offsetMax.y);
                        GridRectTransform.sizeDelta = new Vector2(0, GridRectTransform.sizeDelta.y);
                    }
                }
            }
        }

    }

}