using EvilComic.DataAccess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EvilComic.Common
{
    public static class Extensions
    {
        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="list"> 数据源 </param>
        /// <param name="order"> 排序表达式 </param>
        /// <param name="page"> 第几页 </param>
        /// <param name="size"> 每页记录数 </param>
        /// <param name="count"> 记录总数 </param>
        /// <returns></returns>
        public static IQueryable<T> Pagegation<T,TKey>(this IQueryable<T> list, Expression<Func<T, TKey>> order, int page, int size, out int count)
        {
            count = list.Count();
            return list.Distinct().OrderBy(order).Skip((page - 1) * size).Take(size);
        }

        public static XEImageContext GetXEImageContext(this Xeinformation info, XEDbContext XEDb)
        {
            var query = XEDb.Xeconnection.Include(m => m.DbPath).FirstOrDefault(m => m.ConnectionId == info.ConnectionId);

            if (query == null)
                throw new InvalidOperationException();

            var option = new DbContextOptionsBuilder<XEImageContext>();
            option.UseSqlite("Data Source=" + System.IO.Path.Combine(query.DbPath.Path, query.DbGuid + ".sqlite"));
            return new XEImageContext(option.Options);
        }

        public static bool IsAjaxRequest(this Microsoft.AspNetCore.Http.HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            if (request.Headers != null)
                return request.Headers["X-Requested-With"] == "XMLHttpRequest";
            return false;
        }
    }
}
