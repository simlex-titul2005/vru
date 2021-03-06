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
            var order = new SxOrderItem { FieldName = "Title", Direction = SortDirection.Asc };
            var filter = new SxFilter(page, _pageSize) { Order = order };

            var viewData = _repo.Read(filter);
            if (page > 1 && !viewData.Any())
                return new HttpNotFoundResult();

            var viewModel = new SxPagedCollection<VMService> {
                Collection = viewData,
                PagerInfo= filter.PagerInfo
            };

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

#if !DEBUG
[OutputCache(Duration =900)]
#endif
        [HttpGet, ChildActionOnly]
        public PartialViewResult MainPageServices(int amount=3)
        {
            var order = new SxOrderItem { FieldName = "Title", Direction = SortDirection.Asc };
            var filter = new SxFilter(1, 3) { Order = order };
            
            var viewModel = _repo.Read(filter);

            return PartialView("_MainPageServices", viewModel);
        }
    }
}