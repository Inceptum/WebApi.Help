; (function (window, $, infuser) {

    function HelpPageModel(uri) {
        infuser.defaults.templateSuffix = ".tmpl.html";
        infuser.defaults.templateUrl = "./templates";

        var tableOfContent = ko.observable(),
            items = ko.observableArray(),
            load = function () {
                return $.ajax({
                    url: uri,
                    type: 'GET',
                    contentType: "application/json",
                    accepts: "application/json",
                    dataType: 'json',
                    cache: false
                }).done(function (data) {
                    items(data.items);
                    tableOfContent(data.tableOfContent);
                });
            },
            render = function ($container) {
                ko.applyBindings(this, $container.get(0));
                setupLangBox();
                setupTocNavigation();
                $(window).on('hashchange', onUriHashChanged);
            },
            setupLangBox = function () {
                $('a[data-lang]').click(function (e) {
                    e.preventDefault();
                    var vm = ko.dataFor(this);
                    vm.changeLanguage($(this).data('lang'));
                    return false;
                });
            },
            $selectedTocItem = $([]),
            setupTocNavigation = function () {
                $('.toc').on("click", "a", function (e) {
                    e.stopImmediatePropagation();

                    var $a = $(this);
                    if ($selectedTocItem[0] !== $a[0]) {
                        selectItemInTocTree($a);
                        // We'll handle navigation manually to ensure it always works
                        bringElementOnTop($($selectedTocItem.attr('href')), true);
                    }
                    return true;
                });
            },
            selectItemInTocTree = function ($item) {
                console.log('Selecting:' + $item.attr('id'));
                // Switch selected navigation item
                $selectedTocItem.removeClass('selected');
                $selectedTocItem = $item.addClass('selected');

                // Collapse all non-top level navigarion tree nodes except ones that are parents of selected branch
                $('.toc-list.nested').not($selectedTocItem.parents('.toc-list.nested')).addClass('hidden');

                // Expand nodes from selected branch up to tree's root (required if navigation was done via changing window location, not by clicking on items)
                $selectedTocItem.parents('.toc-list.nested').removeClass('hidden');

                // Expand direct child branch of the selected node
                $selectedTocItem.next('.toc-list.nested').removeClass('hidden');
            },
            bringElementOnTop = function ($elem, animate) {
                if ($elem.length <= 0) return;
                if (animate) {
                    $('html, body').animate({ scrollTop: $elem.offset().top }, 200);
                }
                else {
                    $('html, body').scrollTop($elem.offset().top);
                }
            },
            onUriHashChanged = function () {
                var hash = $.trim(window.location.hash || '');
                if (hash == '') return;

                var $a = $('.toc').find('a[href="' + hash + '"]:first'),
                    $elem = $(hash);
                bringElementOnTop($elem, false);
                selectItemInTocTree($a);
            },
           setCookie = function (cname, cvalue, exdays) {
               var d = new Date();
               d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
               var expires = "expires=" + d.toUTCString();
               window.document.cookie = cname + "=" + cvalue + "; " + expires;
           },
           changeLanguage = function (lang) {
               setCookie('help-page', lang, 365);
               window.location.reload();
           },
           startScrollTracking = function () {
               var timer = null;
               $(window).scroll(function () {
                   if (timer) {
                       window.clearTimeout(timer);
                       timer = null;
                   }
                   var cutoff = $(window).scrollTop();
                   $('a[data-js-anchor]').each(function () {
                       var $anchor = $(this),
                           top = $anchor.offset().top;                       
                       if (top >= cutoff) {
                           var $a = $('.toc').find('a[href="#' + $anchor.attr('id') + '"]:first');
                           if ($selectedTocItem[0] !== $a[0]) {
                               timer = window.setTimeout(function () {
                                   selectItemInTocTree($a);
                                   //bringElementOnTop($anchor, true);
                               }, 500);
                           }
                           return false; // stop iteration
                       }
                       return true;
                   });
               });
           },
           renderedTemplatesCount = 0;

        return {
            tableOfContent: tableOfContent,
            items: items,
            load: load,
            render: render,
            changeLanguage: changeLanguage,
            onRendered: function () {
                // Wait until all items are rendered. There are two templates (header & content) per each item
                if (++renderedTemplatesCount >= 2 * items().length) {
                    window.setTimeout(onUriHashChanged, 50);
                    startScrollTracking();
                }
            }
        };
    }

    window.HelpPageModel = HelpPageModel;

})(window, jQuery, infuser);