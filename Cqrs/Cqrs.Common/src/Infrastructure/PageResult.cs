using System;
using System.Collections.Generic;

namespace Cqrs.Infrastructure
{
    public class PageResult<T>
    {
        public static readonly PageResult<T> Empty = new PageResult<T>();

        private PageResult()
        { }

        public PageResult(int totalRecords, int pageSize, int pageIndex, IEnumerable<T> data)
        {
            this.TotalRecords = totalRecords;
            this.PageSize = pageSize;
            this.PageIndex = pageIndex;
            this.TotalPages = (int)Math.Ceiling((double)totalRecords / (double)pageSize);

            this.Data = data;
        }

        public int TotalRecords { get; private set; }
        public int TotalPages { get; private set; }
        public int PageSize { get; private set; }
        public int PageIndex { get; private set; }
        public IEnumerable<T> Data { get; private set; }
    }
}
