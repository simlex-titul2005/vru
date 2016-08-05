﻿using Dapper;
using SX.WebCore;
using SX.WebCore.Abstract;
using SX.WebCore.Providers;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using vru.Models;
using static SX.WebCore.HtmlHelpers.SxExtantions;

namespace vru.Infrastructure.Repositories
{
    public sealed class RepoServices : SxDbRepository<int, Service, DbContext>
    {
        public override Service[] Read(SxFilter filter, out int allCount)
        {
            var sb = new StringBuilder();
            sb.Append(SxQueryProvider.GetSelectString());
            sb.Append(" FROM D_SERVICE AS ds");

            object param = null;
            var gws = getServicesWhereString(filter, out param);
            sb.Append(gws);

            var defaultOrder = new SxOrder { FieldName = "ds.DateCreate", Direction = SortDirection.Desc };
            sb.Append(SxQueryProvider.GetOrderString(defaultOrder, filter.Order));

            sb.AppendFormat(" OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", filter.PagerInfo.SkipCount, filter.PagerInfo.PageSize);

            //count
            var sbCount = new StringBuilder();
            sbCount.Append("SELECT COUNT(1) FROM D_SERVICE AS ds");
            sbCount.Append(gws);

            using (var conn = new SqlConnection(ConnectionString))
            {
                var data = conn.Query<Service>(sb.ToString(), param: param);
                allCount = conn.Query<int>(sbCount.ToString(), param: param).SingleOrDefault();
                return data.ToArray();
            }
        }

        private static string getServicesWhereString(SxFilter filter, out object param)
        {
            param = null;
            string query = null;
            query += " WHERE (ds.Html LIKE '%'+@html+'%' OR @html IS NULL) ";
            query += " AND (ds.Title LIKE '%'+@title+'%' OR @title IS NULL) ";
            query += " AND (ds.Duration LIKE '%'+@duration+'%' OR @duration IS NULL) ";
            query += " AND (ds.Cost LIKE '%'+@cost+'%' OR @cost IS NULL) ";

            var html = filter.WhereExpressionObject != null && filter.WhereExpressionObject.Html != null ? (string)filter.WhereExpressionObject.Html : null;
            var title = filter.WhereExpressionObject != null && filter.WhereExpressionObject.Title != null ? (string)filter.WhereExpressionObject.Title : null;
            var duration = filter.WhereExpressionObject != null && filter.WhereExpressionObject.Duration != null ? (string)filter.WhereExpressionObject.Duration : null;
            var cost = filter.WhereExpressionObject != null && filter.WhereExpressionObject.Cost != null ? (string)filter.WhereExpressionObject.Cost : null;

            param = new
            {
                html = html,
                title= title,
                duration= duration,
                cost= cost
            };

            return query;
        }

        public override Service GetByKey(params object[] id)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                var data = conn.Query<Service>("dbo.get_service_by_id @serviceId", new { serviceId = id[0] }).SingleOrDefault();
                return data;
            }
        }

        public Service GetByTitleUrl(string titleUrl)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                var data = conn.Query<Service>("dbo.get_service_by_title_url @titleUrl", new { titleUrl = titleUrl }).SingleOrDefault();
                return data;
            }
        }

        public override Service Create(Service model)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                var data = conn.Query<Service>("dbo.add_service @title, @titleUrl, @html, @duration, @cost", new
                {
                    title = model.Title,
                    titleUrl = model.TitleUrl,
                    html = model.Html,
                    duration = model.Duration,
                    cost = model.Cost
                }).SingleOrDefault();

                return data;
            }
        }

        public override void Delete(Service model)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Execute("dbo.del_service @serviceId", new { serviceId = model.Id });
            }
        }

        public override Service Update(Service model)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                var data = conn.Query<Service>("dbo.update_service @serviceId, @title, @titleUrl, @html, @duration, @cost", new
                {
                    serviceId=model.Id,
                    title = model.Title,
                    titleUrl = model.TitleUrl,
                    html = model.Html,
                    duration = model.Duration,
                    cost = model.Cost
                }).SingleOrDefault();

                return data;
            }
        }
    }
}