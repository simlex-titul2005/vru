﻿using SX.WebCore;
using System.Linq;
using System.Web.Mvc;
using vru.Infrastructure.Repositories;
using vru.Models;
using vru.ViewModels;
using static SX.WebCore.HtmlHelpers.SxExtantions;

namespace vru.Controllers
{
    public sealed class ServicesController : BaseController
    {
        private static RepoServices _repo;
        public ServicesController()
        {
            if (_repo == null)
                _repo = new RepoServices();
        }

        private readonly int _pageSize = 10;

#if !DEBUG
[OutputCache(Duration =900)]
#endif

        [HttpGet]
        public ActionResult Index(int page = 1)
        {
            var order = new SxOrder { FieldName = "Title", Direction = SortDirection.Asc };
            var filter = new SxFilter(page, _pageSize) { Order = order };
            var totalItems = 0;
            var data = _repo.Read(filter, out totalItems);
            filter.PagerInfo.TotalItems = totalItems;
            var viewModel = data
                .Select(x => Mapper.Map<Service, VMService>(x))
                .ToArray();

            ViewBag.Filter = filter;

            return View(viewModel);
        }

        [HttpGet]
        public ActionResult Details(string titleUrl)
        {
            var data = _repo.GetByTitleUrl(titleUrl);
            if (data == null)
                return new HttpNotFoundResult();

            var viewModel = Mapper.Map<Service, VMService>(data);
            return View(viewModel);
        }
    }
}