using System.Collections.Generic;

namespace RecyclerView{

    internal class Pool
    {

        int poolSize, cacheSize;
        public Pool(int poolSize, int cacheSize)
        {
            this.poolSize = poolSize;
            this.cacheSize = cacheSize;
        }
        public Queue<IViewHolderInfo> Scrap = new Queue<IViewHolderInfo>();


        public bool IsFull()
        {
            return Scrap.Count >= poolSize;
        }

        public IViewHolderInfo GetFromPool()
        {
            if (Scrap.Count > 0)
            {
                return Scrap.Dequeue();
            }
            else
            {
                return null;
            }
        }


        public IViewHolderInfo Throw(IViewHolderInfo vh)
        {
            if (Scrap.Count < poolSize)
            {
                vh.Status = Status.RECYCLED;
                Scrap.Enqueue(vh);
            }
            else
            {
                vh.Status = Status.RECYCLED;
                IViewHolderInfo recycled = Scrap.Dequeue();
                Scrap.Enqueue(vh);
                return recycled;
            }
            return null;
        }


    }

}