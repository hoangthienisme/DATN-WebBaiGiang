using Microsoft.EntityFrameworkCore;
namespace WebBaiGiang.Models
{
    public class PhanTrang<T> : List<T>
    {
        public int PageIndex { get; private set; }     // Trang hiện tại
        public int TotalPages { get; private set; }    // Tổng số trang

        public PhanTrang(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            this.AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        // Hàm tạo từ IQueryable
        public static async Task<PhanTrang<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PhanTrang<T>(items, count, pageIndex, pageSize);
        }
    }
}
