
var _lang_user = '';
//var page = {_columns: [] }
 var page ={}

    
//{'_title':'aa','_lang':'aa','_langs':'aa','_engname':'youpingshen','_adminflag':'admin','_columns':[{'form':'value1','column':'value2','sort':'value2'}]}
 eval(" page={'_title':'aa','_lang':'aa','_langs':'aa','_engname':'youpingshen','_adminflag':'admin','_columns':[{'form':'value2','column':'value2','sort':'value2'},{'form':'value2','column':'value1','sort':'value1'}] }");
 
 
 eip = {
   windowOpenObj: null,
    combox: {
        create: function (options) {
            options = $.extend({
                obj: $("#combox"), //容器
                type: "combox",  //combox,search。combox必须选中一个，表单域才有值。而search，user输入的就是value
                name: null,   //默认与obj的id相同
                val: null,
                multiple: false,    //允许多行选择
                para: null,      //ajax参数
                userpara: null,  //变动ajax参数
                comma: ', ',
                valueColumn: null,  //值对应的栏位，默认是第一个字段
                displayColumn: null, //用于显示在文本框的字段
                hiddenColumns: [], //['a','b']，哪些字段不显示在表格里
                onchange: null,  //function(obj,value){} 
                url: null,
                load1: false,    //只load一次。
                width: null,
                height: 250
            }, options || {});
            var obj = options.obj.empty().attr("eip-combox", "Y").addClass('l-btn-left l-btn-icon-right')
                .append('<span class="l-btn-icon icon-{0}">&nbsp;</span>'.format(options.type=='combox'?'more1':'search'));
            var name = options.name || obj.attr("id");
            var textName = options.type == "search" ? " name='{0}'".format(name) : "";
            var text = $("<input eip-combo-text=Y style='padding-right:22px;' type=text {0}>".format(textName)).prependTo(obj).click(function () {
                var obj = $(this).closest("span[eip-combox=Y]");
                if ($(this).data('panel').is(":visible"))
                    $(this).data('panel').panel('close');
                else {
                    var panel=eip.panel.open($(this).data('panel'));
                    eip.datagrid.fit($("#eip_combo_grid", panel));
                }
            }).keyup(function (event) {
                var obj = $(this).closest('span[eip-combox=Y]');
                var options = obj.data('options');
                if (!options.url) return alert('url is null.');
                var val = $(this).val();
                if (options.multiple) {
                    var vals = val.split(options.comma);
                    val = vals[vals.length - 1];
                }
                if (val == '') return $(this).data('panel').find('table').empty();
                if (eip.combox.timer)
                    window.clearTimeout(eip.combox.timer);
                if (event.keyCode == 13) load();
                else
                    eip.combox.timer = window.setTimeout(load, 500);
                function load() {
                    eip.combox.load(obj, options.url,[{ name: 'key', value: val }]);
                }
            });
            if (options.type == "combox") obj.append("<select multiple name='{0}' style='display:none'></select>".format(name));            
            eip.combox.fit(obj);
            var panel = eip.panel.create(text, null, "<table style='cursor:default;'></table>", options.width, options.height, true, { cls: "combo-p" });
            obj.data('options', options);
            if (options.url && options.load1)
                eip.combox.load(obj, options.url);
            if (options.val) eip.combox.setVal(obj, options.val, !options.load1);
        },
        load: function (combox, url, para, data, doFunc) {
            (function (combox, url, para, data, doFunc) {
                var options = combox.data('options');
                para = eip.para.extend(para, options.para);
                para = eip.para.extend(para, options.userpara);
                eip.post(url, para, function (data) {                    
                    options.data = data;
                    var panel = $(':text[eip-combo-text=Y]', combox).data('panel');
                    var table = $('table', panel).empty();
                    if (data.length == 0) return doFunc && doFunc(combox, data);
                    table.append('<tr seq=-1><td colspan=10 class=combox><{空}></td></tr>'.lang());
                    var columns = data.columns.filter(function (c) { return $.inArray(c.column, options.hiddencolumns) == -1; });
                    var trs = data.rows.map(function (row,idx) {
                        var tr = ['<tr seq=' + idx + '>'];
                        for (i in columns)
                            tr.push('<td class=combox>' + row[columns[i].column] + '</td>');
                        tr.push('</tr>');
                        return tr.join('');
                    });
                    table.append(trs.join(''));
                    $("tr", table).click(function () {
                        var panel = $(this).closest('div[eip-panel=Y]');
                        var combox = panel.data('obj').closest('span[eip-combox=Y]');
                        var options = combox.data("options");
                        if (!options.multiple) {
                            if (!options.selected) options.selected = $('tr.combox-selected', panel);
                            options.selected.removeClass('combox-selected');
                            options.selected = $(this);
                            panel.panel('close');
                        }
                        $(this).toggleClass('combox-selected');
                        if (options.multiple && $(this).attr('seq') == '-1') $('tr.combox-selected').removeClass('combox-selected');
                        if (options.multiple && options.type == 'search') {
                            var text = $(':text[eip-combo-text=Y]', combox);
                            var vals = text.val().split(options.comma);
                            var row = options.data.rows[$(this).attr('seq')];
                            var valueColumn = options.valueColumn || options.data.columns[0].column;
                            vals[vals.length - 1] = row[valueColumn];
                            text.val(vals.join(options.comma) + options.comma);
                        }
                        else eip.combox.setVal(combox);
                        options.onchange && options.onchange(combox, $('select[name],:text[name]', combox).val());

                    }).hover(function () { $(this).addClass("datagrid-row-over"); }, function () { $(this).removeClass("datagrid-row-over"); });
                    doFunc && doFunc(combox, data);
                });
            })(combox, url, para, data, doFunc);
        },

        empty: function (combox) {
            var panel = $(':text[eip-combo-text=Y]', combox).data('panel');
            $('table', panel).empty();
            $('select', combox).empty();
            $(':text', combox).val('');
        },

        /*vals："123" 或["123","456"] 多个值必须传数组
         *ifLoad:是否要加载数据后再setVal
        */
        setVal: function (combox, vals, ifLoad) {
            (function (combox, vals, ifLoad) {
                var options = combox.data('options');
                if (vals!=undefined && ifLoad == undefined && !options.load1) ifLoad = true;
                if (ifLoad) {
                    if ($.type(vals) != 'array') vals = [vals];                    
                    eip.combox.load(combox, options.url, vals.map(function (val) {
                        return {name:'val',value:val}
                    }), null, function (combox) {
                        setval(combox, vals);
                    });
                }                    
                else setval(combox,vals);
            })(combox, vals, ifLoad);            
            return combox;

            function setval(combox, vals) {
                var options = combox.data("options");
                if (!options.data || options.data.rows.length == 0) return clear(combox);
                var valueColumn = options.valueColumn || options.data.columns[0].column;
                var displayColumn = options.displayColumn || options.data.columns[options.data.columns.length > 1 ? 1 : 0].column;               
                var texts = [], select = $('select[name]', combox).empty();
                var panel = $(':text[eip-combo-text=Y]', combox).data('panel');
                var table = $('table', panel);
                if (vals) {
                    if ($.type(vals) == 'string') vals = [vals];
                    for (i in options.data.rows) {
                        var row = options.data.rows[i];
                        if ($.inArray(row[valueColumn], vals) > -1) {
                            select.append('<option selected>' + row[valueColumn] + '</options>');
                            texts.push(row[displayColumn]);
                            $('tr[seq=' + i + ']', table).removeClass('combox-selected').addClass('combox-selected');
                            if (!options.multiple) break;
                        }
                    }
                } else {
                    vals = '';
                    $('tr.combox-selected[seq!=-1]',table).each(function () {
                        var row = options.data.rows[$(this).attr('seq')];
                        select.append('<option selected>' + row[valueColumn] + '</options>');
                        vals = row[valueColumn];
                        texts.push(row[displayColumn]);
                    });
                }
                var comma = options.multiple ? options.comma : '';
                $(':text[eip-combo-text=Y]', combox).val(options.type == 'combox' ? texts.join(options.comma) + comma : vals + comma);
            }

            function clear(combox) {
                $('select[name]', combox).empty();
                $(':text[eip-combo-text=Y]', combox).val('');
            }
        },

        fit: function (obj) {
            obj.each(function () {
                $(':text', this)._outerWidth(obj.width());
            });
        }
    },

    datagrid: {
        create: function (options) {
            options = $.extend({
                obj: $("#grid"),    //容器
                hiddencolumns: [], //隐藏栏位列表
                lockcolumns: 0, //锁定列数
                columns: {},    //栏位客制化
                sort: false, //是否排序
                sorts: {},  //排序栏位：{'料号':'DESC','组织':'ASC'}
                page: 1,    //加载时显示第几页
                muledit: true,  //允许多行编辑
                detail: null,    //funtion(row){return {url:'form.detail?id=1',html:'<a href=#>工单号</a>',dofunc:function(obj){}};} url、html任选一即可
                clear:false,    //清除重复项
                showfilter: false,   //显示筛选
                shownum: true,  //显示行号
                showbtns: true, //显示工具栏
                showExcelExport:false,//显示汇出按钮
                showcolumns: true,  //显示栏位名称
                showloading: true,  //显示加载框
                showOrderBtns:false, //是否显示调整tr顺序按钮
                fitcolumns: false,   //自适应栏位宽度
                pagesize: 50,   //每页行数
                pagelist: [10, 20, 50, 100, 150, 200],  //可选的每页行数列表
                checkbox: null,  //function (row){return '<input type=checkbox name=id value="{0}" {1}>'.format(row['ID'],'')}
                trcheck: false,  //通过点击tr，让checkbox选中
                trclick: null,   //点击行时执行的函数：function(grid,trs){}
                wrap: false,     //值是否自动换行
                more: null,       //显示在toolbar上的信息。
                dofunc: null,     //function (grid, trs) { } 其中，trs=[trs0,trs1,trs2 ...]
                ajaxType: 'get',    //load时，ajax请求类型
                setup: { lock: [], hide: [], sort: true } //lock:锁定，不可隐藏的栏位；hide：可以隐藏的栏位（show和hide只能设一个）；sort：可以调整顺序（像类似于有统计类别的不固定栏位，就不适合调整顺序）
            }, options || {});
           // var obj = options.obj.attr("eip-datagrid", "Y").empty().append($("#eip_datagrid_tp").html().format(Lang("第{0}/{1}页，共{2}笔", "0", "0", "0"), Lang('汇出设定...'), Lang('栏位隐藏与排序...'), Lang("发Notes"), Lang(options.more)));
            var obj = options.obj.attr("eip-datagrid", "Y").empty().append($("#eip_datagrid_tp").html().format(Lang("第{0}/{1}页，共{2}笔", "0", "0", "0")));

            options.dc = {
                view: $("div.datagrid-view", obj),
                view1: $("div.datagrid-view1", obj),
                view2: $("div.datagrid-view2", obj),
                header1: $("div.datagrid-view1>div.datagrid-header", obj),
                header2: $("div.datagrid-view2>div.datagrid-header", obj),
                body1: $("div.datagrid-view1>div.datagrid-body", obj),
                body2: $("div.datagrid-view2>div.datagrid-body", obj),
                footer1: $("div.datagrid-view1>div.datagrid-footer", obj),
                footer2: $("div.datagrid-view2>div.datagrid-footer", obj)
            };
            var excelBtn = $("#eip_datagrid_excel", obj).linkbutton().click(function (e, showhidden, showSum) {
                var grid = eip.datagrid.getGrid(this);
                var options = grid.data("options");
                if (!options.data) return false;
               //alert(options["exportPara"])
                // alert(options["exportUrl"])
               // options["exportPara"] = para;
                // options["exportUrl"] = url;
                var UrlExport = "";
                if (options.showExcelExport) {

                    UrlExport = PAGE_EXPORTEXECL + "?exportUrl=" + eip.utf8(options["exportUrl"]) + "&eip_page_title=" + eip.utf8(document.title);

                    var DownLoadFilePost = function (options1) {
                        var config = $.extend(true, { method: 'post' }, options1);
                        var $iframe = $('<iframe id="down-file-iframe" />');
                        var $form = $('<form target="down-file-iframe" method="' + config.method + '" />');
                        $form.attr('action', config.url);

                        for (var key in config.data) {
                            // alert(key + "-" + config.data[key]);
                            $form.append('<input type="hidden" name="' + config.data[key].name + '" value="' + eip.utf8(config.data[key].value) + '" />');
                        }
                        $iframe.append($form);
                        $(document.body).append($iframe);
                        $form[0].submit();
                        $iframe.remove();
                    }
                    DownLoadFilePost({ url: UrlExport, data: options["exportPara"] });


                }
                else {
              
                    
                    if (options["exportUrl"].indexOf("?")>0)
                        UrlExport = options["exportUrl"] + "&ExportExecl=Y&eip_page_size=999999&eip_page_title=" + eip.utf8(document.title);
                    else
                        UrlExport = options["exportUrl"] + "?ExportExecl=Y&eip_page_size=999999&eip_page_title=" + eip.utf8(document.title);
                    for (var key in options["exportPara"]) {
                        if (options["exportPara"][key].name != "eip_page_size")
                            UrlExport += "&"+options["exportPara"][key].name +"="+ eip.utf8(options["exportPara"][key].value);
                    }
                     //alert(UrlExport);
                    window.open(UrlExport);
                   // location.href = UrlExport;
                }

                //location.href = PAGE_EXPORTEXECL + "?ExportExecl=Y&exportUrl=" + eip.utf8(options["exportUrl"]) + "&eip_page_title=" + eip.utf8(document.title) + "&eip_rnd=" + Math.random();
            });
           // if (!options.showExcelExport) $("#eip_datagrid_excel").hide();

            //var grid_menu = $('#eip_datagrid_more', obj).menubutton({ hasDownArrow: false, menu: $('#eip_datagrid_more_menu', obj) }).data('menubutton').options.menu.data('eip-btn', excelBtn);
            //$('div[excel-setup]', grid_menu).click(function () {
            //    var excelBtn = $(this).closest('div.menu').data('eip-btn');
            //    var dlg = eip.dialog.create(Lang('汇出到Excel'), '<div style="padding:5px"><input type=checkbox id=eip-datagrid-menu-showall value=Y><label for=eip-datagrid-menu-showall>{显示隐藏栏位}</label></div><div style="padding:5px"><input type=checkbox value=Y id=eip-datagrid-menu-sum><label for=eip-datagrid-menu-sum>{显示求和项}</label></div>'.lang(), 400, 200, {
            //        iconCls: 'icon-excel', maximizable: false,
            //        buttons: [
            //            {
            //                text: Lang('汇出到Excel'), iconCls: 'icon-excel', handler: function () {
            //                    excelBtn.trigger('click', [$('#eip-datagrid-menu-showall:checked', dlg).val(), $('#eip-datagrid-menu-sum:checked', dlg).val()]);
            //                    eip.dialog.close(dlg);
            //                }
            //            }
            //        ]
            //    }, 'style="padding:20px"');
            //});
            //$('div[column-setup]', grid_menu).click(function () {
            //    var grid = eip.datagrid.getGrid($(this).closest('div.menu').data('eip-btn'));
            //    var options = grid.data('options');
            //    var url = options.url;
            //    if (!options.data) return;
            //    if (url.indexOf('?') > -1) url = url.substr(0, url.indexOf('?'));
            //    options.data && eip.columnsSet(options.data.columns, options.setup, url, options.data.mid, function (data) {
            //        eip.datagrid.display(grid, data);
            //    });
            //});

            if (!options.showcolumns) options.dc.header1.add(options.dc.header2).hide();
            $("a[page]", obj).linkbutton().click(function () {
                var grid = $(this).closest("[eip-datagrid=Y]");
                var options = grid.data("options");
                if (!options.page) options.page = 1;
                var max=Math.ceil(options.data.total*1.0/options.pagesize);
                options.page = { '1': 1, '-1': options.page -1, '+1': options.page +1, '0': max }[$(this).attr("page")];
                if (options.page < 1) options.page = 1;
                if (options.page > max) options.page = max;
      
             //   eip.datagrid.load(PAGE_DATAGRID + ".grid_list", grid);
                //eip.datagrid.load(options.pageUrl, grid, options.pagePara, null, null, null, false);
                eip.datagrid.load(options.url, grid, options.lastPara, null, null, null, false);
  
            }).filter("[page='1'],[page='-1']").linkbutton("disable");

            if (options.showOrderBtns) {
                var td = $('<td><div class="pagination-btn-separator"></div></td><td></td>').insertAfter($('#eip-last-page-td', obj));                
                var orderA = [
                    { at: 'top', func: function (tr) { tr.insertBefore($(tr).siblings().eq(0)); } },
                    { at: 'up', func: function (tr) { tr.insertBefore($(tr).prev()); } },
                    { at: 'down', func: function (tr) { tr.insertAfter($(tr).next()); } },
                    { at: 'bottom', func: function (tr) { tr.insertAfter($(tr).nextAll().last()); } }];
                var orderBtns = orderA.map(function (val) { return '<a href=# plain=true at="{at}" iconCls="icon-{at}"></a>'.format({ at: val.at }) }).join('');
                td.last().append(orderBtns);
                $('a', td.last()).linkbutton().click(function () {
                    var btn=$(this);
                    var grid = eip.datagrid.getGrid(btn);
                    var focusTr = grid.data('options').lastClickTrs;
                    if (!focusTr) return;
                    focusTr.each(function () {
                        orderA.filter(function (val) { return val.at == btn.attr('at') })[0].func($(this));
                    });
                });
                
            }

            if (!options.showbtns) $(">tbody>tr:first", obj).hide();
            obj.data("options", options);
            eip.datagrid.fit(obj);

            options.dc.body1.bind("mousewheel DOMMouseScroll", function (e) {
                var dc = $(e.target).closest("table[eip-datagrid=Y]").data("options").dc;
                var e1 = e.originalEvent || window.event;
                var _84 = e1.wheelDelta || e1.detail * (-1);
                dc.body2.scrollTop(dc.body2.scrollTop() - _84);
            });

            options.dc.body2.bind("scroll", function (e) {
                var dc = $(e.target).closest("table[eip-datagrid=Y]").data("options").dc;
                var b1 = dc.view1.children("div.datagrid-body");
                b1.scrollTop($(this).scrollTop());
                var c1 = dc.body1.children(":first");
                var c2 = dc.body2.children(":first");
                if (c1.length && c2.length) {
                    var _85 = c1.offset().top;
                    var _86 = c2.offset().top;
                    if (_85 != _86) {
                        b1.scrollTop(b1.scrollTop() + _85 - _86);
                    }
                }
                dc.view2.children("div.datagrid-header,div.datagrid-footer")._scrollLeft($(this)._scrollLeft());
            });
            $("select.pagination-page-list", obj).change(function () {
                var grid = eip.datagrid.getGrid(this);
                var options = grid.data("options");
                options.pagesize = $(this).val();
                // eip.datagrid.load(PAGE_DATAGRID + ".grid_list", grid, null, 'post');
                eip.datagrid.load(options.url, grid, options.lastPara, null, null, null, false);
            }).empty().append(options.pagelist.map(function (val) { return '<option>' + val + '</option>' }).join('')).val(options.pagesize);

            $("#datagrid-mask-msg", obj).html(Lang("系统正在处理中，请稍候") + "...");

            options["user_columns"] = {};
            options["filter"] = {};
            eip.datagrid.idx = eip.datagrid.idx ? eip.datagrid.idx +1: 1;
            options.idx = eip.datagrid.idx;
            return obj;
        },
        
        /*加载数据
         *dofunc:function(grid,rows){} 在ajax请求完成后，在显示前或后要执行的function。
        */
        load: function (url, grid, para, btns, ajaxType, dofunc, ifSearch) {
            if (!grid || !grid.data('options')) return alert('grid不存在');
            var options = grid.data("options");
            
            //var ifSearch = url.search(PAGE_DATAGRID) == -1;
            if (ifSearch==undefined) ifSearch = true;

            if (url.substr(0, 1) == '.') url = eip.url + url;
            options["scroll_left"] = options.dc.body2.scrollLeft();
            if (options.loading) return alert(Lang("系统正在处理中，请稍候") + "...");
            if (!para) para = [];
 
            Init();
        
           // if (ifSearch && options.showloading) {
            if (options.showloading) {
                var loading = $("#datagrid-mask,#datagrid-mask-msg", grid).show();
                var msg = $("#datagrid-mask-msg", grid)._outerHeight(40)
                msg.css({ marginLeft: (-msg.outerWidth() / 2), lineHeight: (msg.height() + "px") });
            }
 
            (function (grid, options, url, ajaxType, ifSearch, para, dofunc, btns) {
                eip.disabled(btns);
                options.loading = $.ajax({
                    url: url,
                    type: ajaxType ? ajaxType : options.ajaxType,
                    //type: ajaxType,
                    data: para,
                    success: function (data) {
                        eip.disabled(btns, false);
                        options.loading = null;
                        options["exportPara"] = para;
                        options["exportUrl"] = url;
                        $("#datagrid-mask,#datagrid-mask-msg", grid).hide();
                       
                        if (eip.error(data)) return;
                        if ($.type(data) == 'string') return alert(data);                        
                        eip.datagrid.display(grid, data);
                    },
                    error: function (ajaxRequest, textStatus, errorThrown) {
                        
                        options.loading = null;                        
                        AjaxError.apply(this, arguments);
                        $("#datagrid-mask,#datagrid-mask-msg", grid).hide();
                        eip.disabled(btns, false);
                    }
                });
            })(grid, options, url, ajaxType, ifSearch, para, dofunc,btns);

            function Init() {
               // para=eip.para.extend(para, [{ name: 'eip_page_size', value: options.pagesize }])
                //  para = eip.para.extend(para, [{ name: 'eip_rnd', value: Math.random() }])

               if (ifSearch) {
                    options.filter = {};
                    delete options.user_sorts;
                    options.url = url;
                    options.lastPara = para;
                    options.page = 1;
                    para.push({ name: 'eip_page_size', value: options.pagesize });
                    para.push({ name: 'eip_rnd', value: Math.random() });
                    if (options.data && para.filter(function (key) { return key["name"] == "eip_mid"; }).length == 0)
                        para.push({ name: 'eip_mid', value: options.data.mid });
                    var sorts = options.user_sorts || options.sorts, sortJson = [];
                    for (key in sorts) sortJson.push({ name: key, value: sorts[key] });
                    para.push({ name: "eip_sorts", value: JSON.stringify(sortJson) });
                    para.push({ name: "eip_filter", value: JSON.stringify(options.filter) });
                    para.push({ name: 'eip_page', value: options.page });
               } else {
                    
                   var sorts = options.user_sorts, sortJson = [];
                   for (key in sorts) sortJson.push({ name: key, value: sorts[key] });
                   $.each(para, function (n, val) {
                       if (val.name == "eip_sorts")
                           val.value = JSON.stringify(sortJson)
                   });
                   para = eip.para.extend(para, [{ name: 'eip_page', value: options.page }])
                   para = eip.para.extend(para, [{ name: 'eip_rnd', value: Math.random() }])
                   para = eip.para.extend(para, [{ name: 'eip_page_size', value: options.pagesize }])
               }
               
            }
        },

        display: function (grid, data) {
   
            var options = grid.data("options");
            options.data = data;
            options.page = data.page;
            var header1 = options.dc.header1;
            var header2 = options.dc.header2;
            var headers = header1.add(header2);
            var body1 = options.dc.body1;
            var body2 = options.dc.body2;
            var view = options.dc.view;
            data.show_columns = data.columns.filter(function (c) {
                return $.inArray(c.column, options.hiddencolumns) == -1 && c.sort!=null;
            });                                    
            if (!options.fitcolumns) eip.datagrid.creatCss(grid);            
            var sorts = options.user_sorts || options.sorts || {};
            if (options.showcolumns) {
                var heads = data.show_columns.map(function (c, idx) {
                    var sortClass = "datagrid-nosort";
                    if (sorts[c.column]) sortClass = sorts[c.column].toLowerCase() == "asc" ? "datagrid-asc" : "datagrid-desc";
                    return '<td field="{0}" class="{1} {2}"><div class="datagrid-cell datagrid-title"><div title="{3}" style="float:left">{3}</div><div filter=Y class="datagrid-title-filter"><img style="display:none;margin-top:2px;" src="/share/includes/easyui/themes/icons/move-next.gif" /></div></div></td>'.format(c.column, c.cellClass, sortClass, Lang(c.column));
                });
                var heads1 = '';
                if (options.shownum) heads1 += '<td class="datagrid-action datagrid-nosort"></td>';
                if (options.checkbox) heads1 += '<td class="datagrid-action datagrid-nosort"><input type=checkbox id=eip_datagrid_checkall></td>';
                if (options.detail) heads1 += '<td class="datagrid-action datagrid-nosort"></td>';
                heads1 += heads.slice(0, options.lockcolumns).join('');
                $(">div>table>tbody>tr", header1).empty().append(heads1);
                $(">div>table>tbody>tr", header2).empty().append(heads.slice(options.lockcolumns).join(''));
                $("div[title]", headers).each(function () {
                    $(this).css("width", $(this).closest("td").width() - 10);
                });

                //全选
                $("#eip_datagrid_checkall", header1).click(function () {
                    var checked = $(this).filter(":checked").size();
                    var grid = eip.datagrid.getGrid(this);
                    var options = grid.data("options");
                    var checkboxs = $("td.datagrid-action input:checkbox:enabled", options.dc.body1);
                    checkboxs._propAttr("checked", checked ? true : false);

                    $.each(eip.datagrid.getTrs(grid), function (index, value) {
                        $(value).find("td.datagrid-action input:checkbox:enabled").triggerHandler("click");
                    });
                });
            }

            $("div[filter]", headers).click(function () {   
                var td = $(this).closest("td[field]");
                var grid = eip.datagrid.getGrid(td);                
                var options = grid.data("options");
                var sorts = { }, field = td.attr("field");
                var panel = eip.dialog.create(Lang("筛选-") + $("div[title]", td).text(), "<table id=eip_filter_grid></table>", 500, 450, {
                    buttons: [
                        {
                            text:Lang('清除筛选'),iconCls:'icon-cancel',
                            handler: function () {
                                var grid = panel.data("grid");
                                var options = grid.data("options");
                                options.filter[field] && delete options.filter[field];
                                eip.dialog.close(panel);
                                eip.datagrid.load(PAGE_DATAGRID + ".grid_list", grid, null, 'post');
                            }
                        },{
                            text: Lang('确定筛选'), iconCls: 'icon-ok',
                            handler: function () {
                                var grid = panel.data("grid");
                                var filter = grid.data("options").filter;
                                filter[field] = {
                                    not_include: $("[name=not_include]:checked", panel.parent()).val() || "",
                                    filter_op: $("select[name=filter_op]", panel).val() || "",
                                    filter_key: $(":text[name=filter_key]", panel).val() || "",
                                    filter_val: getFilterVal()
                                };
                                eip.dialog.close(panel);
                                eip.datagrid.load(PAGE_DATAGRID + ".grid_list", grid, null, 'post');

                                function getFilterVal() {
                                    var vals = [];
                                    $("[name=filter_val]:checked", panel).each(function () {
                                        vals.push($(this).val());
                                    });
                                    return vals;
                                }
                            }
                        }
                    ]
                });
                $('div.dialog-button', panel.parent()).prepend('<input type=checkbox style="vertical-align:middle" name=not_include id=eip_filter_type value=Y><label style="font-size:12px;vertical-align:middle" for=eip_filter_type>{0}</label>'.format(Lang('不包含模式')));
                panel.data("grid", grid);
                sorts[field] = "asc";
                var filterGrid = eip.datagrid.create({
                    obj: $("#eip_filter_grid", panel),
                    sorts: sorts,
                    showfilter: false,
                    shownum: false,
                    pagesize:200,
                    checkbox: function (row) { return '<input type=checkbox name=filter_val value="{0}">'.format(row[field]) },
                    trcheck: true,
                    ajaxType:'post',
                    dofunc: function (filterGrid) {
                        var row = {};
                        row[field] = Lang("<空白>");
                        var trs = eip.datagrid.insertTr(filterGrid, row);
                        $(":checkbox", trs).val("");
                        var fields = {
                            string: "<input type=text name=filter_key placeholder='{0}' style='width:400px'>".format(Lang('输入关键字搜寻')),
                            number: "<select name=filter_op><option>=</option><option>></option><option>>=</option><option><</option><option><=</option><option><></option></select>&nbsp;<input type=text name=filter_key>",
                            datetime: "<select name=filter_op><option>=</option><option>></option><option>>=</option><option><</option><option><=</option></select>&nbsp;<input type=text name=filter_key size=9 onclick=SelectDate(this,'yyyy-MM-dd')></input>"
                        };
                        var options = filterGrid.data("options");
                        var fieldType = options.data.columns.filter(function (key) { return key.column == field })[0]["type"];
                        row[field] = fields[fieldType];
                        var trs = eip.datagrid.insertTr(filterGrid, row);
                        if (fieldType == "number")
                            $(":text[name=filter_key]", trs).blur(function () {
                                if ($(this).val() != '' && !eip.check.float($(this).val(), true))
                                    $(this).focus().val('');
                            });
                        $(":checkbox", trs).remove();
                        init();

                        function init() {
                            var options = grid.data("options");
                            var filter = options.filter[field];
                            if (!filter) return false;
                            if (filter.not_include == "Y") $(":checkbox[name=not_include]", panel.parent())._propAttr("checked", true);
                            $("select[name=filter_op]", panel).val(filter.filter_op);
                            $(":text[name=filter_key]", panel).val(filter.filter_key);
                            $(":checkbox[name=filter_val]", panel).each(function () {
                                if($.inArray($(this).val(),filter.filter_val)>-1)
                                    $(this)._propAttr("checked", true);
                            });
                        }
                    }
                });
                eip.datagrid.load(PAGE_DATAGRID + ".distinct", filterGrid, [{ name: "eip_mid", value: options.data.mid }, { name: "field", value: field }, { name: "eip_filter", value: JSON.stringify(options.filter) }]);
            });
            $("td[field]", headers).hover(
                function () {
                    $(this).addClass("datagrid-header-over");
                    var options = eip.datagrid.getGrid(this).data("options");
                    if (!options.showfilter) return false;
                    var img = $("div[filter]>img", this).show();
                    $("div[title]", this).css("width", "").css("width", $(this).width() - img.parent().outerWidth() - 10);
                }, function () {
                    $(this).removeClass("datagrid-header-over");
                    $("div[filter]>img", this).hide();
                    $("div[title]", this).css("width", "").css("width", $(this).width() - 10);
                }).each(function () {
                    $(this).resizable({
                        handles: "e", disabled: ($(this).attr("resizable") ? $(this).attr("resizable") == "false" : false), minWidth: 25
                        , onStartResize: function (e) {
                            options.resizing = true;
                            headers.css("cursor", $("body").css("cursor"));
                            if (!options.proxy) {
                                options.proxy = $("<div class=\"datagrid-resize-proxy\"></div>").appendTo(view);
                            }
                            options.proxy.css({ left: e.pageX - $(view).offset().left - 1, display: "none" });
                            setTimeout(function () {
                                if (options.proxy) {
                                    options.proxy.show();
                                }
                            }, 500);
                        }, onResize: function (e) {
                            options.proxy.css({ left: e.pageX - $(view).offset().left - 1, display: "block" });
                            return false;
                        }, onStopResize: function (e) {
                            headers.css("cursor", "");
                            $(this).css("height", "");
                            var field = $(this).attr("field");
                            if (!options.user_columns[field]) options.user_columns[field] = {};
                            options.user_columns[field]["width"] = $(this)._outerWidth()-7;
                            $(this).css("width", "");
                            options.proxy.remove();
                            options.proxy = null;
                            options.resizing = false;
                            eip.datagrid.creatCss(grid);
                            options.lastClickTrs && options.lastClickTrs.trigger('click');
                            eip.datagrid.fit(grid);
                        }
                    });
                }).filter(function () {
                    return options.filter[$(this).attr('field')];
                }).css("color", "blue");
            $("div[title]", headers).click(function () { eip.datagrid.sort($(this).closest("td")); });

            var trs1 = data.rows.map(function (row, idx) {
                return eip.datagrid.createTr(grid, idx, row, true);
            });
            trs1 = $(">div>table>tbody", body1).empty().append(trs1.join('')).children();
            var trs2 = data.rows.map(function (row, idx) {
                return eip.datagrid.createTr(grid, idx, row);
            });
            trs2 = $(">table>tbody", body2).empty().append(trs2.join('')).children();
            trs1.each(function () {
                $(this).data("tr", trs2.filter(':eq({0})'.format($(this).index())));
            });
            trs2.each(function () {
                $(this).data("tr", trs1.filter(':eq({0})'.format($(this).index())));
            });
            eip.datagrid.addTrTrigger(trs1.add(trs2));
            $('a.easyui-linkbutton', grid).linkbutton();
            $(':text[date=Y]', grid).click(function () { SelectDate(this, 'yyyy-MM-dd'); });
            $(':text[datetime=Y]', grid).click(function () { WdatePicker({el:this,dateFmt:'yyyy-MM-dd HH:mm:00'}); });
            

            if (data.sum) {
                var srow = eip.datagrid.emptyRow(grid, data.sum);
      
                data.rows.push(srow);
                var footer1 = $(eip.datagrid.createTr(grid, data.rows.length - 1, srow, true)).attr('sum', 'Y');
                var footer2 = $(eip.datagrid.createTr(grid, data.rows.length - 1, srow)).attr('sum', 'Y');
                $('tbody', options.dc.footer1).empty().append(footer1).find('td.datagrid-action').empty();
                $('tbody', options.dc.footer2).empty().append(footer2);
                $('td[field]', footer1.add(footer2)).each(function () {
                    if ($(this).text() == '') {
                        $(this).text(Lang('合计'));
                        return false;
                    }
                });
            }

            $("#eip_grid_page_info", grid).html(Lang("第{0}/{1}页，共{2}笔", data.page,Math.ceil(data.total * 1.0 / options.pagesize, "0"), data.total));
            var pageBtns = $("a[page]", grid);
            pageBtns.filter("[page='1'],[page='-1']").linkbutton(options.page == 1 ? "disable" : "enable");
            pageBtns.filter("[page='+1'],[page='0']").linkbutton(data.page == Math.ceil(data.total * 1.0 / options.pagesize, "0") ? "disable" : "enable");
            if (options.dofunc || options.clear || options.wrap) var trs = eip.datagrid.getTrs(grid);
            options.dofunc && options.dofunc(grid, trs);

            if (options.clear) {
                var trLen = trs.length;
                for (var i = 1; i < trLen; i++) {
                    var rowp = data.rows[i - 1], row = data.rows[i];
                    $('td[field]', trs[i]).each(function () {
                        var field = $(this).attr('field');
                        if (rowp[field] == row[field])
                            $(this).empty();
                        else
                            return false;
                    });
                }
            }

            if (options.wrap) {
                var trLen = trs.length;
                for (var i = 0; i < trLen; i++) {
                    var tr = trs[i], max = Math.max(tr.first().height(), tr.last().height());
                    tr.css("height", max);
                }
            }

            eip.datagrid.fit(grid);
            options.dc.body2.triggerHandler("scroll");
            options.dc.body2.scrollLeft(options["scroll_left"]);
        },

        emptyRow: function (grid, row) {
          //  var aa= JSON.parse(row);
            var options = eip.datagrid.getOptions(grid);
     
            var data = options.data, crow = {};
            if (!data) return row || {};
            for (i in data.columns)
                crow[data.columns[i].column] = '';
            if (row) crow = $.extend(crow, JSON.parse(row));
            return crow;
        },

        createTr: function (grid,seq,row,ifLeft) {
            var options = grid.data("options");
            var rownum = (options.page - 1) * options.pagesize + seq + 1;
            if (rownum < 1) rownum = "";
            var className = options.wrap && seq != -999 ? 'datagrid-wrap' : 'datagrid-nowrap';
            var tds = ifLeft ? options.data.show_columns.slice(0, options.lockcolumns) : options.data.show_columns.slice(options.lockcolumns);
            var initIdx = ifLeft ? 0 : options.lockcolumns;
            for (col in row)
                if (typeof row[col] == 'number') row[col] = Math.round(row[col] * 1000000) / 1000000.0;
            tds = tds.map(function (c,idx) {
                var formatter = options.columns[c.column];
                var val = formatter && formatter.formatter ? formatter.formatter(row) : row[c.column];
                if (!val) val = '';
                return '<td field={0} class="datagrid-{1}-{2} {3}">{4}</td>'.format(c.column, options.idx, idx + initIdx, className, val);
            });
            var tr = '<tr seq="{0}" class="datagrid-row">'.format(seq);
            if (ifLeft) {
                if (options.detail) tds.unshift('<td class="datagrid-action" style="height:29px;"><span class="datagrid-row-expander datagrid-row-expand" style="display:inline-block;width:16px;height:16px;cursor:pointer;"></span></td>');
                if (options.checkbox) tds.unshift('<td class="datagrid-action" style="height:29px;">{0}</td>'.format(options.checkbox(row)));
                if (options.shownum) tds.unshift('<td class="datagrid-action datagrid-td-rownumber" style="height:29px;">{0}</td>'.format(rownum));
            }
            tds.unshift(tr);
            tds.push('</tr>');
            return tds.join('');
        },

        creatCss: function (grid) {
            $('style', grid).remove();
            var options = grid.data("options");
            var totalWidth = 0;
            var viewWidth = options.dc.view.width() - 25;
            if (options.fitcolumns && viewWidth>0) {
                var totalWidth = 0;
                for (i in options.data.show_columns)
                    totalWidth += options.data.show_columns[i].width;
            }            
            var style = options.data.show_columns.map(function (c, idx) {
                c.cellClass = 'datagrid-{0}-{1}'.format(options.idx, idx);
                var user_column = options.user_columns[c.column];
                var width = !user_column && totalWidth > 0 ? c.width * 1.0 / totalWidth * viewWidth : c.width;
                return '.{0}{width:{1}px;}'.format(c.cellClass, user_column ? user_column["width"] : width);
            });
            style.unshift('<style type="text/css">');
            style.push('</style>');
            grid.prepend(style.join('\n'));
        },

        getGrid: function (obj) {
            obj = eip.tojq(obj);
            return obj.closest("table[eip-datagrid=Y]");
        },

        getOptions: function (obj) {
            return eip.datagrid.getGrid(obj).data("options");
        },

        //根据obj获取row或者rows。若obj是tr里面的对象，或者传递了seq参数，则返回row；否则返回rows
        getData: function (obj, seq) {
            obj = eip.tojq(obj);
            var grid = eip.datagrid.getGrid(obj);
            var data = eip.datagrid.getOptions(grid).data;
            if (!data) return;
            var tr = obj.closest('tr[seq!=""]', grid);
            if (tr.size()>0 && (seq == undefined || seq == null)) {
                seq = parseInt(tr.attr('seq'));
                if (seq < 0) seq = null;
            }
            if (seq != undefined && seq != null) return data['rows'][seq];
            return data['rows'];
        },

        getTr: function (obj) {
            obj = eip.tojq(obj);
            var tr = obj.filter("tr").length > 0 ? obj : obj.closest('tr');
            return tr.add(tr.data("tr"));
        },

        getTrs: function (grid) {
            var options = grid.data("options");
            var trs = [];
            var trs1 = $(">div>table>tbody>tr",options.dc.body1);
            var trs2 = $(">table>tbody>tr", options.dc.body2);
            trs1.each(function (i) {
                trs.push($(this).add(trs2.filter(":eq(" + i + ")")));
            });
            if (options.data && options.data.sum)
                trs.push($('tr', options.dc.footer1.add(options.dc.footer2)));
            return trs;
        },
        addTrTrigger: function (tr) {
            tr.hover(function () {
                $(this).add($(this).data("tr")).addClass("datagrid-row-over");
            }, function () {
                $(this).add($(this).data("tr")).removeClass("datagrid-row-over");
            }).click(function () {
                var grid = $(this).closest("table[eip-datagrid=Y]");
                var options = grid.data("options");                
                if (options.lastClickTrs) {
                    options.lastClickTrs.removeClass('eipgrid-row-selected');
                    if (!options.wrap) options.lastClickTrs.css("height", "25");
                }
                var trs = $(this).add($(this).data("tr")).addClass('eipgrid-row-selected');
                var max = Math.max(trs.first().height(), trs.last().height());
                trs.css("height", max);
                options.dc.body2.triggerHandler("scroll");
                options.lastClickTrs = trs;
                options.trclick && options.trclick(grid, trs);
                if (options.checkbox && options.trcheck && !$(event.srcElement).is(":checkbox")) {
                    var checkbox = $(">td.datagrid-action :checkbox", trs);
                    checkbox._propAttr("checked", !checkbox._propAttr("checked"));
                }
            });
            $('span.datagrid-row-expander', tr).click(function () {
                var grid = eip.datagrid.getGrid(this);
                var options = grid.data('options');                
                var tr = eip.datagrid.getTr(this);
                var ifshow = $(this).hasClass('datagrid-row-expand') && (tr.next().size() == 0 || tr.next().is('[seq]'));
                var newClass = ifshow ? 'datagrid-row-collapse' : 'datagrid-row-expand';
                $(this).removeClass('datagrid-row-collapse datagrid-row-expand').addClass(newClass);
                if (!ifshow) return tr.next().remove();
                tr.after('<tr style="display: table-row;"><td colspan="999" style="border-right:0;">&nbsp;</div></td></tr>');
                var trs = tr.next();
                var div = $('<div class="ddv panel-body panel-body-noheader panel-body-noborder" style="padding-bottom: 10px;" title=""></div>').appendTo(tr.last().next().children());               

                var row = eip.datagrid.getData(tr);
                var detail = options.detail(row || {}, div) || {};
                setHtml(trs, div, detail.html || '');
                if (detail.url)
                    (function (trs, div, url) {
                        eip.get(url, function (data) {
                            setHtml(trs, div, data);
                        });
                    })(trs, div, detail.url);

                function setHtml(trs,div,html) {
                    div.empty().append(html);                    
                    detail.dofunc && detail.dofunc(div);
                    var max = Math.max(trs.first().height(), trs.last().height());
                    trs.css("height", max);
                    options.dc.body2.triggerHandler("scroll");
                }                
            });
        },

        //obj：可以是trs,tr,任意一个obj；fields：['字段1', '字段2']
        edit: function (obj, fields) {
            if ($.type(obj) != 'array') obj = [eip.datagrid.getTr(obj)];
            for (i in obj) {                
                var tr = eip.datagrid.getTr(obj[i]);
                grid = eip.datagrid.getGrid(tr);
                eip.datagrid.addEditor(grid, tr, fields);
            }
        },

        insert: function (grid, tr, fields) {
            if (!eip.datagrid.mayEdit(grid)) return;
            var tr = eip.datagrid.insertTr(grid);
            eip.datagrid.addEditor(grid,tr,fields);
            return tr;
        },

        reset: function (tr, url, para, doFunc, ajaxType, showloading) {
            tr = (tr instanceof jQuery) ? tr : $(tr);
            tr = eip.datagrid.getTr(tr);
            var grid = eip.datagrid.getGrid(tr);            
            var data = eip.datagrid.getData(grid);
            var seq = parseInt(tr.attr("seq"));

            var reset = function (tr, seq,row) {
                $(":text[eip-combo-text=Y]", tr).each(function () {
                    eip.panel.destory($(this).data("panel"));
                });
                var tr1 = $(eip.datagrid.createTr(grid, seq, row, true)).insertAfter(tr.first());
                var tr2 = $(eip.datagrid.createTr(grid, seq, row)).insertAfter(tr.last());
                tr.remove();
                tr1.data('tr', tr2);
                tr2.data('tr', tr1);
                tr = tr1.add(tr2);
                $("a.easyui-linkbutton", tr).linkbutton({});
                var dofunc = grid.data("options").dofunc;
                dofunc && dofunc(grid, [tr]);
                eip.datagrid.addTrTrigger(tr);
                return tr;
            };

            if (seq == -1 && !url) return tr.remove();
            if (url) {
                var ajax = ajaxType == "post" ? eip.post : eip.get;
                ajax(url, para, function (row) {
                    if ($.type(row) == "string") {
                        doFunc && doFunc(row);
                        return;
                    }
                    row = row.rows || row;
                    if ($.isArray(row)) {
                        if (row.length == 0) return tr.remove();
                        row = row[0];
                    }
                    if (seq == -1) {
                        seq = data.length;
                        data.push(row)
                    } else
                        data[seq] = row;
                    reset(tr, seq, row);
                    doFunc && doFunc(tr);
                }, showloading);
            } else
                return reset(tr, seq, data[seq]);
        },

        addEditor: function (grid, tr, fields) {
         
            var options = eip.datagrid.getOptions(grid);
            var row = tr.attr("seq") == "-1" ? {} : eip.datagrid.getData(tr);
            tr.attr("edit", "Y");
            for (key in options.columns) {
                if (fields && $.inArray(key, fields) == -1) continue;
                var editor = options.columns[key]['editor'], fhtml = null;
                if ($.type(editor) == 'function') editor = editor(row, tr);
                if (!editor) continue;
                var value = editor.value;
                if (!value) value = row[editor.field || key];
                if (!value) value = '';
                var td = $('td[field="' + key + '"]', tr).empty();
                switch (editor.type) {
                    case 'text':
                        fhtml = "<input type=text value='" + value + "' name='" + editor.name + "' id='" + editor.name + "' " + (editor.more ? editor.more(row) : "") + "/>";
                        break;
                    case 'date':
                        fhtml = "<input type=text name='" + editor.name + "' id='" + editor.name + "' " + (editor.more ? editor.more(row) : "") + " onclick=\"SelectDate(this, 'yyyy-MM-dd')\" value=\"" + value + "\"/>";
                        break;
                    case 'datetime':
                        fhtml = "<input type=text name='" + editor.name + "' id='" + editor.name + "' " + (editor.more ? editor.more(row) : "") + " onclick=\"WdatePicker({el:this,dateFmt:'yyyy-MM-dd HH:mm:00'})\" value=\"" + value + "\"/>";
                        break;
                    case 'select':
                        var fhtml = $.isFunction(editor.html) ? editor.html(row) : editor.html;
                        (fhtml instanceof jQuery ? fhtml : $(fhtml)).val(value);
                        break;
                    case 'combox':
                        var combox = $("<span " + (editor.more ? editor.more(row) : "") + "></span>").appendTo(td)._outerWidth(td.width());
                        var eoptions = $.isFunction(editor.options) ? editor.options(row,tr) : editor.options;
                        eoptions = $.extend(eoptions, { obj: combox, width: td.width() });
                        eip.combox.create(eoptions);
                        if (value != '') eip.combox.setVal(combox, value);
                        break;
                    case "func":
                        fhtml = editor.func(row, tr);
                        if (fhtml instanceof jQuery) {
                            td.append(fhtml);
                            fhtml = null;
                        }
                        break;
                }
                if (fhtml) {
                    td.append(fhtml);
                    $('>:text,>select', td)._outerWidth(td.width());                    
                }
            }
            $(':text[date=Y]', tr).click(function () { SelectDate(this, 'yyyy-MM-dd'); });
            $(':text[datetime=Y]', tr).click(function () { WdatePicker({ el: this, dateFmt: 'yyyy-MM-dd HH:mm:00' }); });
            
            $('a.easyui-linkbutton', tr).linkbutton();
            tr.first().trigger('click');
        },

        mayEdit: function (grid) {
            var grid = eip.datagrid.getGrid(grid);
            var options = grid.data('options');
            if (!options) return false;
            if (!options.data)
                return alert(Lang("请先查询资料。")) && false;
            if (options.muledit) return true;
 
            if (eip.datagrid.getTrs(grid).filter("[edit=Y]").size() > 0)
                return alert(Lang("对不起，不能同时编辑多行，谢谢！")) && false;
            return true;
        },

        //gridTr：grid或者tr，若是grid，则在最前面添加一行；若是tr，则在tr后面添加一行
        insertTr: function (gridTr,row) {
            if (gridTr.is('table')) {
                var grid = gridTr;
                var tr = null;
            } else {
                grid = eip.datagrid.getGrid(gridTr);
                tr = gridTr;
            }
            if (!row) row = {};
            var options = grid.data("options");
            var tr1 = $(eip.datagrid.createTr(grid, -1, row, true)).prependTo($(">div>table>tbody", options.dc.body1));
            var tr2 = $(eip.datagrid.createTr(grid, -1, row)).prependTo($(">table>tbody", options.dc.body2));
            tr1.data("tr", tr2);
            tr2.data("tr", tr1);
            eip.datagrid.addTrTrigger(tr1.add(tr2));
            return tr1.add(tr2);
        },

        sort: function (td) {
            var grid = eip.datagrid.getGrid(td);
            var options = grid.data("options");
            if (!options.sort || options.resizing) return;
            if (!window.event.ctrlKey || !options.user_sorts) options.user_sorts = {};
            if (td.hasClass("datagrid-asc")) options.user_sorts[td.attr("field")] = "desc";
            else if (td.hasClass("datagrid-desc")) options.user_sorts[td.attr("field")] = "asc";
            else options.user_sorts[td.attr("field")] = "desc";
           // alert("aaa")

            eip.datagrid.load(options.url, grid, options.lastPara, null, null, null, false);
           // eip.datagrid.load(PAGE_DATAGRID + ".grid_list", grid);
        },

        fit: function (grids) {
            grids.each(function () {
                var grid = $(this);
                var options = grid.data('options');
                var dc = options.dc;
                var all = grid.add(dc.view).add(dc.view1).add(dc.view2).add(dc.body1).add(dc.body2).css("height", "");
                all.add(dc.header1).add(dc.header2).css("width", "");
                grid.width(grid.parent().width()).height(grid.parent().height());
                var container = $(">tbody>tr:eq(1)>td", grid);
                var viewWidth = container.width();
                var viewHeight = container.height();
                var view = dc.view;
                var view1 = dc.view1;
                var view2 = dc.view2;
                view.width(viewWidth);
                var view1tb = dc.header1.children("div.datagrid-header-inner").show();
                view1.width(view1tb.find("table").width());
                view2.width(viewWidth - view1._outerWidth());
                view1.children()._outerWidth(view1.width());
                view2.children()._outerWidth(view2.width());
                var otherHeight = dc.header2.is(':visible') ? dc.header2._outerHeight() : 0;
                if (options.data && options.data.sum) {
                    options.dc.footer1.add(options.dc.footer2).show();
                    var fheight = Math.max(options.dc.footer1._outerHeight(), options.dc.footer2._outerHeight());
                    otherHeight += fheight;
                } else options.dc.footer1.add(options.dc.footer2).hide();
                var borderHeight = container.outerHeight() - container.height();
                var minHeight = container._size("minHeight") || "";
                var maxHeight = container._size("maxHeight") || "";
                view1.add(view2).children("div.datagrid-body").css({ marginTop: 0, height: viewHeight - otherHeight, minHeight: (minHeight ? minHeight - borderHeight - otherHeight : ""), maxHeight: (maxHeight ? maxHeight - borderHeight - otherHeight : "") });
                view.height(view2.height());                
                if (options.data && options.data.rows.length > 0) $('div.datagrid-body-line', view2).hide();
                else $('div.datagrid-body-line', view2)._outerWidth($('table.datagrid-htable', view2)._outerWidth()).show();
                if (options.fitcolumns && options.data) eip.datagrid.creatCss(grid);
            });
        }
        
    },
    columnsSet: function (columns, setup, url, mid, dofunc) {
        var dlg = eip.dialog.create(Lang('栏位隐藏与排序'), "<div fit=true><div id=toolbar data-options=\"region:'north',collapsible:false\" style='padding:1px;height:30px;background:#eee'><a href=# page=1 data-options=\"iconCls:'pagination-first',plain:true\" /a><a href=# page=-1 data-options=\"iconCls:'pagination-prev',plain:true\" /a><a href=# page=+1 data-options=\"iconCls:'pagination-next',plain:true\" /a><a href=# page=0 data-options=\"iconCls:'pagination-last',plain:true\" /a></div><div title={显示的栏位列表} data-options=\"region:'center',collapsible:false\"><select style='width:100%;height:367px;' id=show multiple=multiple></select></div><div data-options=\"region:'east',collapsible:false\" title={隐藏的栏位列表} style='width:300px;'><select id=hide multiple=multiple style='width:100%;height:367px;'></select></div></div></div>".lang(), 600, 500, {
            maximizable: false, buttons: [
                { text: Lang('删除设定'), iconCls: 'icon-cancel', handler: function () { $(this).parent().children(':last').trigger('click',[true]) } },
                {
                    text: Lang('保存设定'), iconCls: 'icon-save', handler: function (e,del_flag) {
                        var para = [{ name: 'page', value: url }, { name: 'eip_mid', value: mid }];
                        if (del_flag) {
                            para.push({ name: 'show', value: '' });
                            para.push({ name: 'hide', value: '' });
                        } else
                            $('option', dlg).each(function () {
                                var parent_id = $(this).parent().attr('id');
                                if (setup.sort != null && setup.sort == false && parent_id == 'show') return;
                                para.push({ name: parent_id, value: $(this).attr('value') });
                            });
                        eip.post(PAGE_DATAGRID + ".columnset", para, function (data) {
                            dofunc && dofunc(data, para);
                        });                        
                        eip.dialog.close(dlg);
                    }
                }                
            ]
        });
        $("div[fit=true]", dlg).layout();
        $("#toolbar>a", dlg).linkbutton({ disabled: setup.sort != null && !setup.sort }).click(function () {
            if (setup.sort !=null && !setup.sort) return;
            var list = $('#show', dlg), selected = $('option:selected', list);
            if (selected.size() != 1) return alert(Lang('请在左边列表选中一行。'));
            switch ($(this).attr('page')) {
                case '1':
                    selected.prependTo(list);
                    break;
                case '-1':
                    if (selected.index() != 0) selected.insertBefore(selected.prev());
                    break;
                case '+1':
                    if (selected.index() < $('option', list).length - 1) selected.insertAfter(selected.next());
                    break;
                case '0':
                    selected.appendTo(list);
                    break;
            }
        });

        var show = $('#show', dlg).append(createOption(true));
        var hide = $('#hide', dlg).append(createOption(false));
        $('option', dlg).dblclick(function () {
            if ($(this).attr('lock') == 'Y') return;
            var select = $(this).closest('select'), ifLeft = select.attr('id') == 'show';
            var other = $("#" + (ifLeft ? 'hide' : 'show'), dlg).val('');
            $(this).appendTo(other);
        });
        function createOption(ifShow) {
            return columns.filter(function (c) { return ifShow && c.sort != null || !ifShow && c.sort == null }).map(
                function (c) {
                    var color = setup.lock && $.inArray(c.column, setup.lock) > -1 || setup.hide && setup.hide.length > 0 && $.inArray(c.column, setup.hide) == -1 ? 'gray' : 'black', lock = color == 'gray' ? 'Y' : 'N';
                    return Lang('<option value="' + c.column + '" sort="{0}" style="color:{1}" lock={2}>{3}</option>', c.sort, color, lock, c.column);
                }
                ).join('');
        }
    },
    fit: function (obj) {
        obj = eip.tojq(obj);
        obj._outerWidth(obj.parent().width());
        obj._outerHeight(obj.parent().height());
        return obj;
    },
    utf8: function (val) {
        return encodeURIComponent(val);
    },
    url: null,
    check: {
        float: function (text, ifAlert) {
            if (!$.isNumeric(text)) {
                if (ifAlert) alert(Lang("\"{0}\" 不是数值！", text));
                return false;
            }
            return true;
        }
    },
    para: {
        /* 创建要传递给ajax请求的参数
        ** elements：要生成参数的jquery对象或者json对象，其中json对象要是如下格式：{name1:value1,name2:value2,……}
        ** [include]：如果elements是json对象，此参数为要包含的键，用数组表示，比如["a","b"]
        ** [exclude]：如果elements是json对象，此参数为要排除的键，用数组表示，比如["a","b"]
        ** eg: var para = eip.para.create($("[name]"));
        **     var para = eip.para.create($("[name]"),null,["remark","create_date"]);把所有有name属性的表单域生成为参数，但不包括name="remark"和"create_date"的
        **     var para = eip.para.create({user_name:"张三",sex:"男",remark:"班长"},null,["remark"]);
        */
        create: function (elements, include, exclude) {
            if (!elements) return;
            var disEles = elements.filter(':disabled').attr("disabled", false), para = [];
            if (elements instanceof jQuery)
                para = elements.serializeArray();
            else {
                $.each(elements, function (key, val) {
                    if (!include && !exclude || include && $.inArray(key, include) > -1 || exclude && $.inArray(key, exclude) == -1)
                        para.push({ name: key, value: val });
                });
            }
            disEles.attr("disabled", true);
            return para;
        },

        /* 在target上追加more参数，若有相同的，则覆盖，并返回追加结果
        ** target：原参数
        ** more：要追加的参数
        ** return：array
        ** eg: var para=eip.para.extend([{name:"user_name",value:"张三"}],[{name:"sex",value:"男"}]);
        */
        extend: function (target, more) {
            if (!more) return target;
            if (!more.length) more = [more];
            if (more.length == 0) return target;
            else if (!target || target.length == 0) return more;

            var ftarget = target.slice(0);
            for (var j = 0; j < more.length; j++) {
                for (var i = 0; i < ftarget.length; i++)
                    if (more[j].name == ftarget[i].name)
                        ftarget.splice(i--, 1);
            }
            return ftarget.concat(more);
        },

        /*
        *datatype可以是'array','string'。默认是'string'
        *para=[{name:'username',value:'nali'},{name:'username',value:'rainxie'}]
        *如果name='username',datatype='array'，返回['nali','rainxie']
        *                    datatype=null,返回 'nali,rainxie'
        */
        val: function (para, name, dataType) {
            var data = dataType == "array" ? [] : "";
            for (var i = 0; i < para.length; i++)
                if (para[i].name == name) {
                    if (dataType == "array")
                        data.push(para[i].value);
                    else {
                        if (data != "") data += ",";
                        data += para[i].value;
                    }
                }
            return data;
        }
    },

    /*发送消息给主窗口或其他窗口
    action：消息名称，可以自己编，比如 '删除'、'刷新grid'等
    data：要传递的参数，类似{id:1}
    tabname：窗口名称
    */
    postmessage: function (action, data, tabname, id, dofunc) {
        if (window.parent == window) return;
        if (!eip._message) eip._message = {};
        if (dofunc) {
            id = 'M' + Math.random();
            eip._message[id] = dofunc;
        }
        window.parent.postMessage({ action: action, data: data, id: id, tabname: tabname }, '*');
    },

    /*接受消息并处理
    action：要处理的消息名称
    dofunc：处理函数，会把发送消息的data传递给该函数，如 function(data){}
    */
    execmessage: function (action,dofunc) {
        if (!action || !dofunc) return;
        eip._eip_message[action] = dofunc;
    },

    //tab: {
    //    add: function (url, title) {
 
    //        if (window.AddTab) {
    //            alert("aa")
    //            return p.AddTab(title, url)
    //        };
          
    //        eip.postmessage('addtab', { url: url, title: title });
    //    },

    //    //title不传，即为关闭当前tab
    //    close: function (title) {
    //        if (window.CloseTab) return window.CloseTab(title);
    //        eip.postmessage('closetab', { title: title });
    //    },

    //    open: function (url) {
    //        eip.postmessage('window_open', { url: url });
    //    }
    //},

    tab: {
        "add": function (url, title) {
            var p = parent;
            if (!p.GetTabTitle) p = opener;
            if (!p.GetTabTitle) return;
            title = title == undefined ? p.GetTabTitle(url) : title;
            p.AddTab(title, url);
        },
        "close": function (title) {
            if (this.parent == this) return;
            if (parent.CloseTab) {
                parent.CloseTab(title);
                return true;
            }
        },
        "window": function (title) {
            if (this.parent == this) return;
            return parent.GetTabWindow(title);
        },
        "currTabName": function () {
            if (this.parent == this) return;
            return parent.GetCurrTabName();
        }
    },
    /*
    读和写每个user的配置参数，类似cookie，只是必须登录才有效
    write：会发出ajax请求，并执行刷新配置参数的事件
    read：从default.aspx的json里读取配置参数。只有在tab和弹出窗口里才有效
    */
    ini: {
        read: function (key) {
            if (!eip._ini) return '';
            var val = eip._ini[key];
            return val || "";
        },
        write: function (key, value) {
            if (!eip._ini) return;
            eip._ini[key] = value;
            eip.postmessage('newini', { key: key, value: value });
        }
    },
    login_do: function () {
        eip.post("/Admin/Admin/LoginCheck", [{ name: "UserName", value: $("[name=UserNameLogin]").val() }, { name: "WhCode", value: $("[name=WhCodeLogin]").val() }, { name: "PassWord", value: $("[name=PassWordLogin]").val() }], function (jsonObj) {
            $.procAjaxData(jsonObj, function () {
                if (jsonObj.Data != null) {
                    //$("[name=WhCodeLogin]").parent().show();
                    var select = "<option></option>";
                    for (var i = 0; i < jsonObj.Data.length; i++) {
                        select += "<option value='{0}'>{1}</option>".format(jsonObj.Data[i].val, jsonObj.Data[i].key);
                    }
                    $("[name=WhCodeLogin]").html(select);
                    $("[name=WhCodeLogin]").focus();
                }

            }, function () { });
        })

    }
     ,
    login: function () {
        //eip.dialog.create('Login', 'Login', 500, 400);
        var options = {
            "width": 450,
            "height": 250,
            "closable":false
        };
        var dlg = eip.dialog.create("登录", "<div id=loginAG><div form='Y' id='form_loginAG'>"
                             + "<input type=\"text\" left=\"140\" name=\"UserNameLogin\" hint=\"账号\" /><br/>"
                             + "<input type=\"password\" left=\"140\" name=\"PassWordLogin\" hint=\"密码\" /><br/>"
                             + "<select name=\"WhCodeLogin\" left=\"140\" style=\"width:120px;\" hint=\"仓库\"  onchange=\"eip.login_do()\"><option> </option></select><br/>"
                             + "</div></div>", 500, 400,options)

            //窗体工具栏按钮 begin
        eip.layout.create($('#loginAG'), [{
            obj: $('#form_loginAG'),
                buttons: [
                    {
                        text: "登录", icon: "save", click:
                            function () {
                                if ( $("[name=UserNameLogin]").val() == '') {
                                    $("[name=UserNameLogin]").focus();
                                    return false;
                                }
                                if ( $("[name=PassWordLogin]").val() == '') {
                                    $("[name=PassWordLogin]").focus();
                                    return false;
                                }
                                eip.login_do();


                            }

                    },
                    {
                        text: "关闭", icon: "no", click:
                        function () {                     
                            eip.dialog.close(dlg);
                        }

                    }
                ]
            }])
        $("[name=UserNameLogin]").focus();

    },

    //打开Notes员工信息，userName可以是中英文、工号
    "openemployee": function (userName, orgCode) {
        eip.get('/common/workflow.employee', [{ name: "username", value: userName }, { name: "orgcode", value: orgCode || "" }], function (data) {
            if (data == "") return alert(Lang("对不起，没找到“{0}”的员工信息", userName));
            var html = "<iframe scrolling=no frameborder=0 src='http://notesap.iei.com.tw/global/ieiworldaddressbook.nsf/webQuery/" + data + "?OpenDocument'/>";
            var dlg = eip.dialog.create(Lang("员工信息"), html, 1000, 600);
            eip.fit($('iframe', dlg));
        });
    },

    workflow: {

        pass: function (ids, success, obj) {
            eip.disabled(obj || success);
            eip.get("/common/workflow.pass_?flow_id=" + ids, function (val) {
                success && success(val);
                eip.disabled(obj || success);
            });
        },

        /*  
        doFunc：     签核完成后，要执行的函数；
        dataFunc：   要改变驳回下拉菜单选项时用到，改变格式：{ORDER_NAME : "", MAIL_ADDRESS : "", ORDER_ID : "", APPROVE_BY : "", APPROVE_RESULT:"Y"}
        requestFunc：点击驳回按钮要执行的函数，若返回true，则继续执行通用的驳回程序           
        */
        reject: function (flowID, doFunc, dataFunc, requestFunc) {
            var data = eip.go("/common/workflow.flowlist?flow_id=" + flowID);
            if (dataFunc) data = dataFunc(data);
            if (data.length == 0) return alert(Lang("对不起，可以驳回的列表为空，无法驳回"));
            var flowName = data[0]["FLOW_NAME"];
            var tableID = data[0]["TABLE_ID"];
            var selectVal = data[0]["ORDER_ID"];
            var title = Lang("驳回 “{0}”，单号：{1}", flowName, tableID);
            var html = ("<div form=Y style='margin:10px'><select hint='{驳回到}' name='v_reject_order_id'></select><span id='v_mails' hint='通知人'></span><input type=text hint='{驳回意见}' name=v_remark></div>").lang();
            var dlg = eip.dialog.create(title, html, 402, 200, {
                buttons: [
                    {
                        text: Lang("驳回"), iconCls: "icon-undo", handler:
                          function () {
                              var reject_order_id = dlg.find("[name=v_reject_order_id]").val();
                              var reject_remark = dlg.find("[name=v_remark]").val();
                              if (reject_order_id == "" || reject_remark == "") return alert(Lang("对不起， “驳回到” 和 “驳回意见” 都不能为空"));
                              var url = "/common/workflow.reject";
                              var para = eip.para.create(dlg.find("[name=v_reject_order_id],[name=v_remark],[name=v_mails]"));
                              para = eip.para.extend(para, { name: "v_flow_id", value: flowID });
                              if (!requestFunc || requestFunc(para))
                                  eip.get(url, para, doFunc);
                              eip.dialog.close(dlg);
                          }
                    },
                    { text: Lang("取消"), handler: function () { eip.dialog.close(dlg); } }
                ]
            });
            eip.hint.create($('div[form]', dlg),null, 100, 260);
            eip.combox.create({
                obj: $('#mails', dlg),
                type: 'search',
                url: '/common/comboxs.mail'
            });
            var rejectSelect = $('select[name=v_reject_order_id]', dlg);
            eip.form.createOptions(rejectSelect,
                data.filter(function (row) { return row['APPROVE_RESULT'] == 'Y' })
                .map(function (row) {
                    return { 'ORDER_ID': row["ORDER_ID"], 'ORDER_NAME': row['ORDER_NAME'] + '(' + ["APPROVE_BY"] + ')' }
                }), true);
            rejectSelect.change(function () {
                var val = $(this).val();
                for (var i = 0; i < data.length; i++)
                    if (data[i]["ORDER_ID"] == val) {
                        eip.combox.setVal($("[id=mails]", dlg), (data[i]["MAIL_ADDRESS"] || '') + ",");
                        break;
                    }
            }).val(selectVal);
        },

        history: function (flowID) {
            if (!flowID) return;
            var dlg = eip.dialog.create(Lang("签核历史"), "<table id=eip-grid></table>", 700, 450);
            eip.datagrid.create({
                obj: $('#eip-grid', dlg),
                pagesize: 200,
                sorts: { "签核时间": "ASC" },
                hiddencolumns: ["ORDER_ID", "ID", "USER_NAME", "POW", "上级主管"],
                columns: {
                    '签核时间': { formatter: function (row) { if (row['签核时间'].substr(0, 4) == '2999') return ' '; else return row['签核时间'] } },
                    "签核人": {
                        formatter: function (row) {
                            return "<a class=blue href=# onclick=\"eip.openemployee('" + row["签核人"] + "')\">" + row["签核人"] + "</a>";
                        }, editor: {
                            type: "combox", options:
                                function (row, td) {
                                    return { url: '/common/comboxs.users' };
                                }
                        }
                    }, "状态": {
                        formatter: function (row) {
                            if (row["ID"] != null && (row["签核人"] == row["USER_NAME"] || page._adminflag || row["USER_NAME"] == row["上级主管"]))
                                return "<a href=# class=easyui-linkbutton plain=true iconCls='icon-edit' onclick=\"eip.datagrid.edit($(this).closest('tr'))\"></a>";
                        }, editor: {
                            type: "func", func:
                                function (row) {
                                    return "<a href=# class=easyui-linkbutton plain=true iconCls='icon-save' onclick=\"eip.datagrid.reset($(this).closest('tr'),'/common/workflow.save?id=" + row["ID"] + "&user_name='+$('select[name]',$(this).closest('tr')).val())\"></a>"
                                        + "<a href=# class=easyui-linkbutton plain=true onclick=\"eip.datagrid.reset($(this).closest('tr'))\" iconCls='icon-cancel'></a>";
                                }
                        }
                    }
                }
            });
            eip.datagrid.load('/common/workflow.history?flow_id=' + flowID, $('#eip-grid', dlg));
            return;
        }
    },

    error: function (data) {
        if ($.type(data) == 'string' && data.substr(0, 10) == '[EIP-ERROR') {
            var error = null,error_desc=null, sys_error = null, translateBtn = '';
            if (data.substr(0, 18) == '[EIP-ERROR-SYSTEM]') sys_error = data.substr(18);
            else {
                error = data.substr(11, 3);
                error_desc = data.substr(11);
            }
            if (error == '401')
                eip.login();
            else if (error == '402')
                error = alert('ERROR:\n\n' + Lang('权限不足') + '\n\n');
            else if (sys_error) {
                if (eip.login_dlg) return;
                if (page._adminflag) {
                    var arrorA = sys_error.split("\n"), ifError;
                    for (i in arrorA) {
                        color = "black"
                        if (arrorA[i].trim().substr(0, 6) == 'File "')
                            color = 'gray';
                        else if (arrorA[i].trim().substr(0, 6) == 'Traceb')
                            color = 'black';
                        else if (arrorA[i].search("Error: ") > -1) {
                            color = 'red';
                            var tran_q = arrorA[i];
                            var translateBtn = '<br><a href=# id=eip-transBtn>翻译</a>&nbsp;<span id=eip-trans style="color:red"></span>';
                            ifError = true;
                        } else if (ifError)
                            color = 'red';
                        else
                            color = 'blue';
                        arrorA[i] = '<font color=' + color + '>' + arrorA[i].replace(/\s/g, "&nbsp;") + '</font>';
                    }
                }
                var body = page._adminflag ? arrorA.join('<br>') + translateBtn
                    : '<img src="/share/includes/easyui/themes/icons/no.png"> {0}<hr><div style="float:right"><input type=text style="width:200px;" id=description placeholder="{1}">&nbsp;<input type=button value="{2}"></div>'.format(Lang('对不起，发生系统错误。'), Lang('简略说明'), Lang('发送错误给管理员'));
                eip.login_dlg = eip.dialog.create(Lang('系统错误'), body, (page._adminflag ? 800 : 600), (page._adminflag ? 400 : 250), {
                    iconCls: 'icon-no', maximizable: false, buttons: [
                        {
                            text: '&nbsp;' + Lang('确定') + '&nbsp;', handler: function () {
                                eip.dialog.close(eip.login_dlg)
                            }
                        }], onClose: function () { eip.login_dlg = null; }
                }, 'style="padding:' + (page._adminflag ? 10 : 40) + 'px"');
                $('#eip-transBtn', eip.login_dlg).linkbutton().click(function () {
                    eip.get('/common/funcs.translate?q=' + eip.utf8(tran_q), function (data) {
                        var html = data.trans_result ? data.trans_result[0].dst : '由于网络或服务器问题，无法翻译。';
                        $('#eip-trans', eip.login_dlg).html(html);
                    });
                });
                $(':text', eip.login_dlg).focus();
                $(':button', eip.login_dlg).click(function () {
                    eip.post('/common/send_mail', [{ name: 'to', value: "IT" }, { name: 'title', value: "系统错误：" + document.URL }, { name: 'body', value: $('#description', eip.login_dlg).val() + "\n\n" + sys_error }], function (data) {
                        if (data == 'Y') {
                            alert(Lang('发送成功'));
                            eip.dialog.close(eip.login_dlg);
                        }
                    });
                });
            } else
                alert('ERROR:\n\n' + Lang(error_desc) + "\n\n");
            return true;
        }
        return false;
    },

    //同步发出ajax请求，并返回一个字符串，para为要传递的参数，可以用eip.para.create创建，type为请求类型（get,post）
    go: function (url, para, success,obj, type) {
        if (!url || url == '') return console.error('eip.go的url参数为空');
        eip.disabled(obj || success);
        if (url.substr(0, 1) == '.') url = eip.url + url;
        var args = eip.varadd(arguments, ['string', 'array', 'function', 'object', 'string']);
        url = args[0], para = args[1], success = args[2], obj = args[3], type = args[4];
        var fpara = [{ name: 'eip_rnd', value: Math.random() }].concat(para && $.type(para) == 'array' ? para : []), data = null;
        $.ajax({
            async: false, url: url, data: fpara, type: type || 'get', success: function (fdata) {
                eip.disabled(obj, false);
                if (eip.error(data)) return;
                data = fdata;
                success && success(fdata);
            }
        });
        if (!eip.error(data)) return data;
    },

    get: function (url, para, success, obj, showloading) {
        if (!url || url == '') return console.error('eip.go的url参数为空');
        if (url.substr(0, 1) == '.') url = eip.url + url;
        var args = eip.varadd(arguments, ['string', 'array', 'function', 'object', 'boolean']);
        url = args[0], para = args[1], success = args[2], obj = args[3], showloading = args[4];
        var fpara = [{ name: 'eip_rnd', value: Math.random() }];
        if (para && $.type(para) == 'array')
            fpara = fpara.concat(para);
        eip.disabled(obj || success);
        if (showloading) eip.showloading();
        return $.get(url, fpara, function (data) {
            eip.disabled(obj, false);
            if (showloading) eip.showloading(true);
            if (eip.error(data)) return;
            if ($.type(para) == 'function') success = para;
            success && success(data);
        });
    },

    post: function (url, para, success, obj, showloading) {
        if (!url || url == '') return console.error('eip.go的url参数为空');
        if (url.substr(0, 1) == '.') url = eip.url + url;
        var args = eip.varadd(arguments, ['string', 'array', 'function', 'object', 'boolean']);
        url = args[0], para = args[1], success = args[2], obj = args[3], showloading = args[4];
        eip.disabled(obj || success);
        if (showloading) eip.showloading();
        return $.post(url, para, function (data) {
            eip.disabled(obj, false);
            if (showloading) eip.showloading(true);
            if (eip.error(data)) return;
            if ($.type(para) == 'function') success = para;
            success && success(data);
        });
    },

    showloading: function (hide) {
        if (hide) {
            $("#eip-mask,#eip-mask-msg").hide();            
        } else {
            var loading = $("#eip-mask,#eip-mask-msg").show();
            var msg = $("#eip-mask-msg")._outerHeight(40);
            msg.css({ marginLeft: (-msg.outerWidth() / 2), lineHeight: (msg.height() + "px") });
        }
    },

    disabled: function (obj, flag) {
        if ($.type(obj) != 'object') return;
        if (!(obj instanceof jQuery)) obj = $(obj);
        if (obj.size() == 0) return;
        if (obj.is(':button')) flag == false ? obj.removeAttr('disabled') : obj.attr('disabled', true);
        else if (obj.is('a.l-btn')) obj.linkbutton(flag == false ? 'enable' : 'disable');
        eip['_disabled'] = obj;
    },

    varadd: function (args, types) {        
        var arglen=args.length;typelen = types.length, vars = [];
        j = 0;
        for (i = 0; i < arglen; i++)
            for (j; j < typelen;j++) {
                if (args[i] == null || args[i] == undefined || $.type(args[i]) == types[j]) {
                    vars.push(args[i]);
                    j++;
                    break;
                } else vars.push(null);
            }
        if (vars.length > typelen) return alert('参数个数或者参数类型不对！');
        return vars;
    },

    layout: {
        /*obj：容器
        toolbars：为表单设定工具栏
        resize:当布局的size改变时，调用的函数：function(panel){}。 panel为布局里面的小容器，你的对象都放在这小容器里
               注意：请用$('#a1',panel) 来获取你的对象
        */
        create: function (obj, toolbars, resize) {
            console.log(obj);
            if (!obj || obj.length < 1) return alert("layout对象为null，调用eip.layout失败。");
            if (!toolbars) toolbars = [];
            if ($.type(toolbars) != 'array') toolbars = [toolbars];
            obj.children().each(function () {
                var div = that = $(this);
                if (that.is('table'))
                    div = $('<div data-options="region:\'center\',split:true"></div>').append(that).appendTo(obj);
                else if (!that.attr('region'))
                    that.attr({ 'data-options': "region:\'north\',split:true" }).css({ height: 'auto' });
            });
            for (var i in toolbars) {
                var header = $("<header style='padding:0px'></header>").prependTo(toolbars[i].obj);
                eip.toolbar.create($.extend(toolbars[i], { obj: header }));
            }
            obj.layout({ fit: true });
            $('>div>div[form=Y]', obj).each(function () {
               eip.hint.create($(this));
            });
            $('div.panel-body', obj).each(function () {
                $(this).data('eip-resize', []);
                $(this).data('panel').options.onResize = function (w, h) {
                    eip.datagrid.fit($('table[eip-datagrid=Y]', this));
                    var resizes = $(this).data('eip-resize');
                    for (i in resizes) resizes[i]($(this));
                    resize && resize($(this));
                }
            });
            return obj;
        }
    },

    hint: {

        //type：只读、编辑，默认为编辑模式
        create: function (form, type, left, right) {
            var tempDiv = $('div[eip-hide=Y]', form);
            form.attr('form', 'Y').css({ "padding": "2px" });
            if (!left) left = form.attr('left');
            if (!right) right = form.attr('right');
            if (type == '只读') form.attr('只读', 'Y');
            else if (type == '编辑') form.removeAttr('只读');
            type = form.is('[只读]') ? '只读' : '编辑';
            if (tempDiv.size() == 0)
                tempDiv = $('<div eip-hide=Y style="display:none"></div>').appendTo(form);

   
            $("[hint]", form).appendTo(tempDiv);
            //form.children(':not(div[eip-hide=Y])').remove();

            var hints = tempDiv.children().each(function (idx) {
                if (!$(this).is('[sort]'))
                    $(this).attr({ sort: idx * 100, osort: idx * 100 });
                eip.hint.add(form, $(this), left, right, type);
            });

            eip.combox.fit(hints.filter("[eip-combox=Y]"));
            if (!form.attr('id')) {
                eip.hint.fit(form);
                return form;
            }
            eip.form.columnset(form);
        },

        add: function (form, obj, left, right, type) {
            var orgObj = obj;
            if (!left) left = obj.attr('left');
            if (!right) right = obj.attr('right');
            left = nvl(left, 68), right = nvl(right, 130);
            if (!obj.is('[sort]'))
                obj.attr({ sort: 1000, osort: 1000 });
            var sort = obj.attr('sort');
            var osort = obj.attr('osort');
            if (type == '编辑' && obj.is('[只读]')) type = '只读';

            var size = eip.getAttr(obj, { left: left, right: right }), l = parseInt(size.left), r = parseInt(size.right);
            var className = obj.attr('color') ? ' hint-' + obj.attr('color') : '';
            requiredStr = obj.is('[必填]') ? '<span requiredspan=Y style="color:red">*</span>' : '';
            var div = $("<div sort=" + sort + " osort=" + osort + " hintdiv='" + obj.attr("hint") + "' class='hint" + className + "' style='width:" + (l + r) + "px'><div class='hint-title-c' title='" + Lang($(this).attr("hint")).replace(/(<([^>]+)>)/ig, '') + "' style='width:" + (l - 10) + "px'><span class='hint-title' style='width:" + (l - 10) + "px'>" + requiredStr + Lang(obj.attr("hint")) + "</span></div></div>").appendTo(form);

            if (type == '只读')
                obj = $('<span></span>').appendTo(div).css('width', r).attr({
                    eip_hint: obj.attr('hint'),
                    eip_field: obj.attr('field'),
                    eip_display: obj.attr('display')
                }).text(obj.attr('eip_display_text') || '');
            else if (obj.is(':checkbox,:radio')) obj = $('<span></span>').append(obj).appendTo(div);

            obj.attr({ 'pop': orgObj.attr('pop'), 'popurl': orgObj.attr('popurl') });
           
            if (obj.is('[pop]') || obj.is('[popurl]')) {
                if (type == '只读') obj.addClass('link');
                eip.pop.call(orgObj, obj);
            }

            if (type == '只读') return obj;

            obj.css("width", obj.is('select,span[eip-combox=Y]') ? r + 2 : r).appendTo(div);
            if (obj.is('select[link],span[eip-combox=Y][link]')) {
                eip.form.linkSelect(obj);
            }
            obj.filter('[date=Y]').add($('[date=Y]', obj)).unbind('click.hint').bind('click.hint', function () { SelectDate(this, 'yyyy-MM-dd'); });
            obj.filter('[datetime=Y]').add($('[datetime=Y]', obj)).unbind('click.hint').bind('click.hint', function () { WdatePicker({ el: this, dateFmt: 'yyyy-MM-dd HH:mm:00' }); });
 
            if (obj.attr('ini')) {
                var iniVal = eip.ini.read(obj.attr('ini'));
                if (obj.val() == '' && iniVal != '') obj.val(iniVal);
                obj.unbind('change.ini').bind('change.ini', function () {
                    eip.ini.write($(this).attr('ini'), $(this).val());
                });
            }
        },

        //显示表单域 hints:['需求日','承诺日']
        show: function (form, hints) {
            $(hints.map(function (f) { return 'div[hintdiv="' + f + '"]:not([eip-hide])' }).join(','), form).removeAttr('eip-hide').show();
            eip.hint.fit(form);
        },

        hide: function (form, hints) {
            $(hints.map(function (f) { return 'div[hintdiv="' + f + '"]' }).join(','), form).removeAttr('eip-hide').hide();
            eip.hint.fit(form);
        },

        refresh: function (form) {
            $('div[hintdiv]', form).each(function () {
                var ele = $('[hint]', this), div = $(this);
                $('span[requiredspan=Y]', div).remove();
                if (ele.is('必填')) div.children(':first').prepend('<span requiredspan=Y style="color:red">*</span>');
            });
        },

        fit: function (form) {
            var p = form.closest("div.panel-body");
            if (p.size() == 0) return;
            var c = p.closest("div.layout");
            var oldHeight = p.panel('panel').outerHeight();
            p.panel('resize', { height: 'auto' });
            var newHeight = p.panel('panel').outerHeight();
            c.layout('resize', {
                height: (c.height() + newHeight - oldHeight)
            });
            eip.datagrid.fit($('table[eip-datagrid=Y]', c));
        }
    },

    toolbar: {
        create: function (options) {
            options = $.extend({
                obj: null,
                buttons: null,
                menubtns: null,
                menu:true,
                title: null,
                links: null
            }, options || {});
            var obj = options.obj.empty().css('display', 'inline-block'), buttons = options.buttons, title = options.title, links = options.links;
            if (obj.size() < 1) return alert("obj不存在，不能调用eip.toolbar.create");

            var btnDiv = $('<div style="flow:left;display:inline-block;padding:3px;background:#f4f4f4;border-right:1px solid #95B8E7"></div>').appendTo(obj);
            if (buttons)
                for (var i in buttons) eip.toolbar.add($.extend(buttons[i], { obj: btnDiv }));
            if (options.menu) {
                var menubtns = options.menubtns || [], addSep = -1;
                if (obj.closest('div[form=Y][id]').size() > 0) {
                    if (menubtns.length > 0) {
                        menubtns.push({ text: "-" });
                        addSep = 1;
                    }
                    menubtns.push({
                        text: "栏位隐藏与排序...", icon: 'column_set', click: function () {
                            var form = $(this).closest('div.menu').data('eip-btn').closest('div.panel').find('div[form=Y]');
                            var formid = form.attr('id').toUpperCase();
                            var columns = [];
                            $('div[hintdiv]', form).each(function (i) {
                                var obj = $(this);
                                columns.push({ column: obj.attr('hintdiv'), sort: obj.attr('sort') == '' ? null : parseInt(obj.attr('sort')) });
                            });
                            eip.columnsSet(columns, {}, eip.url + '?' + form.attr('id'), '', function (data, para) {
                                if (data != 'Y') return alert(data);
                                para = para.filter(function (p) { return p.name == 'show' || p.name == 'hide' });
                                if (para.length == 2 && para[0]['value'] == '' && para[1]['value'] == '')
                                    var columns = [];
                                else {
                                    columns = para.map(function (p, idx) {
                                        return { column: p.value, sort: p.name == 'hide' ? null : idx, FORM: formid };
                                    });
                                }
                                
                                page._columns = page._columns.filter(function (c) { return c.FORM != formid }).concat(columns);
                                eip.form.columnset(form);
                            });
                        }
                    });
                }
                //if (page._help) {
                //    if (menubtns.length > 0 && addSep == -1)
                //        menubtns.push({ text: "-" });
                //    menubtns.push({ text: "帮助...", icon: 'help' });
                //}
                if (menubtns.length > 0) {
                    menubtns = {
                        icon: 'menu', hasDownArrow: false, click: menubtns
                    };
                    eip.toolbar.add($.extend(menubtns, { obj: btnDiv }));
                }
            }

            if (options.title) obj.append('<div style="flow:left;display:inline-block;padding:3px;margin-left:5px;font-size:12px"><b>' + Lang(options.title) + '</b></div>');
            if (options.links) {
                var linkdiv = $('<div style="flow:left;display:inline-block;"></div>').appendTo(obj);
                for (var i in options.links) {
                    var btn = options.links[i];
                    $('<a href="#"' + (document.title == btn.text ? ' class=c8' : '') + ' data-options="plain:true' + (btn.icon ? ',iconCls:\'icon-' + btn.icon + '\'' : '') + '">' + Lang(btn.text) + '</a>').linkbutton().appendTo(linkdiv)
                        .data('link',btn)
                        .click(function () {
                            var link = $(this).data('link');
                            eip.tab.add(link.link, link.text);
                        });
                }
            }
            $('<div class=panel-tool><a href="javascript:void(0)" class="panel-tool-collapse panel-tool-expand"></a></div>').appendTo(obj);
        },

        add: function (options) {
            options = $.extend({
                obj: null, //菜单容器
                text: "",
                icon: null,
                hasDownArrow: true, //是否显示箭头
                click: null, //可以为: "list('1')"； list； [{text:"设置",icon:"config",click:"Setup()"}]。为数组时表示下拉菜单。
                show:true,
                width: 150  //为下拉菜单的宽度
            }, options || {});
            var obj = options.obj, text = options.text, icon = options.icon, click = options.click;
            if (options.html)
                return obj.append(options.html);
            if (options.custom)
                obj.append(options.custom);
            else if ($.type(click) == 'array') {
                var id = "eip-menu-" + Math.round(Math.random() * 1000000, 0);
                var btn = $('<a href="#" data-options="menu:\'#' + id + '\'' + (icon ? ',iconCls:\'icon-' + icon + '\'' : '') + '">' + text + '</a>').appendTo(obj);
                var mm = $('<div id="' + id + '" style="width:' + options.width + 'px;"></div>').appendTo(obj).data('eip-btn', btn);
                for (var i in click) {
                    if (click[i].text == '-') {
                        mm.append(' <div class="menu-sep"></div>');
                        continue;
                    }
                    var func = click[i].click;
                    var div = $('<div data-options="' + (click[i].icon ? 'iconCls:\'icon-' + click[i].icon + '\'' : '') + ($.type(func) == 'string' ? ' onclick=' + func : '') + '">' + Lang(click[i].text) + '</div>').appendTo(mm);
                    if ($.type(func) == "function") div.click(func);
                }
                btn.menubutton({ hasDownArrow: options.hasDownArrow });                
            } else {
                if (text == '-') {
                    obj.append('<a class="l-btn eip-separator"></a>');
                } else {
                    var btn = $('<a href="#" data-options="plain:true' + (icon ? ',iconCls:\'icon-' + icon + '\'' : '') + ($.type(click) == 'string' ? ' onclick=' + click : '') + '">' + Lang(text) + '</a>').linkbutton().appendTo(obj);
                    if ($.type(click) == "function") btn.click(click);
                }
            }
            if (!options.show) btn.hide();
        }, getBtn: function (obj, text, icon) {
            var btns = $([]);
            if (text) search(text, 'text');
            if (icon) search(icon, 'icon');
            return btns;

            function search(texts, key) {

                var filter = {
                    text: function () { return $(">span>span:first", this).text() == Lang(text); },
                    icon: function () { return $(">span>span:eq(1)", this).hasClass("icon-" + text); }
                }

                if ($.type(texts) == 'string') texts = [texts];
                for (i in texts) {
                    var text = texts[i];
                    btns = btns.add($("a", obj).filter(filter[key]));
                }
            }
        }
    },
    form: {
        createOptions: function (selects, data, notAddEmpty) {
            selects.each(function (idx) {
                var select = $(this).empty();
                if (!notAddEmpty) {                    
                    var emptyText = $(this).attr('empty') ? $(this).attr('empty') : '';
                    select.append('<option value="">' + Lang(emptyText) + '</option>');
                }                
                var key = select.attr('field') ? select.attr('field') : select.attr('hint');
                var rows = data[key] || data;
                if (!rows || $.type(rows) != 'array') return;
                for (var i in rows) {
                    var row = rows[i], val = null, display = null;
                    for (key in row) {
                        if (val == null) val = row[key];
                        display = row[key]
                    }
                    select.append("<option value='{0}'>{1}</option>".format(val, display));
                }
            });
            return selects;
        },

        /*
        模拟html的form提交，可以上传文件
        eles：要上传的jquery对象，可以包含文件对象
        more：更多参数，如，{g_id:1}
        dofunc：上传成功，要执行的函数，会传递后台结果到该函数
        */
        postForm: function (url,eles,more,dofunc) {
            var para = new FormData();
            eles.each(function () {
                para.append($(this).attr('name'),
                    $(this).attr('type') == 'file' ?
                        this.files[0] : $(this).val());
            });
            if (more)
                for (key in more)
                    para.append(key, more[key]);

            $.ajax({
                url: url,
                type: 'post',
                data: para,
                cache: false, contentType: false, processData: false,
                success: function (data) {
                    dofunc && dofunc(data);
                }
            });
        },

        /*
        触发file表单域的选择文件事件
        event：对象产生的事件
        fileObj：file表单域对象
        dofunc：文件选择后，要调用的函数，会传递文件名到该函数
        */
        fileClick: function (event,fileObj,dofunc) {
            event.stopImmediatePropagation();
            fileObj.unbind('change').change(function () {
                dofunc && dofunc($(this).val());
            }).click().val('');
        },

        /* 联动菜单
         * fromSelect：必选
         * sync:是否同步，默认false
         */
        linkSelect: function (fromSelect, toSelect, para, url, notAddEmpty, sync) {

            (function (fromSelect, toSelect, para, url, notAddEmpty, sync) {
                var ifSelect = fromSelect.is('select');
                if (ifSelect) {
                    fromSelect.unbind("change.link").bind("change.link", onchange).trigger('change.link');
                } else {
                    var options = fromSelect.data('options');
                    options.onchange = function (obj) {
                        onchange.call(obj);
                    };
                    options.onchange(fromSelect);
                };

                function onchange(event, ifSync) {
                    var obj = $(this);
                    var ifSelect = obj.is('select');
                    var pval = ifSelect ? $(this).val() : $('input[name],select[name]', obj).val();
                    var ftoSelect = toSelect;
                    if (!ftoSelect) {
                        eval('var options=' + $(this).attr('link'));
                        ftoSelect = options.select;
                        para = options.para;
                        if ($.type(para) == 'function') para = para();
                    }

                    var fpara = [{
                        name: ifSelect ? $(this).attr('name') : $('input[name],select[name]', obj).attr('name'),
                        value: pval
                    }];
                    if (para) fpara = fpara.concat(para);
                    ftoSelect.each(function (i) {
                        var select = $(this);
                        if (select.is('span[eip-combox]')) {
                            eip.combox.empty(select);
                            select.data('options').userpara = fpara;
                        } else
                            select.empty();

                        if (pval == '' || pval == null || select.is('span[eip-combox]')) {
                            if (select.is('[link]')) eip.form.linkSelect(select);
                            return;
                        }

                        var furl = url ? url : $(this).attr('url');
                        if ($.type(furl) == "array") furl = furl[i];
                        var ajax = ifSync || sync ? eip.go : eip.get;                        
                        ajax(furl, fpara, function (data) {
                            eip.form.createOptions(select, data, notAddEmpty);
                            select.val(select.attr('eip_value'));
                            if (select.is('[link]')) {
                                eip.form.linkSelect(select);
                            }
                        });
                    });
                };

            })(fromSelect, toSelect, para, url, notAddEmpty, sync);
        },

        val: function (eles, vals) {
            var form = eles.first().closest('div[form=Y]');
            var toSelects = $();
            var links = eles.filter('[link]').each(function () {
                eval('var options=' + $(this).attr('link'));
                toSelects.add(options.select);
            });

            eles.each(function () {
                var ele = $(this);
                var field = ele.attr('field') || ele.attr('hint');
                var val = vals[field] == undefined ? '' : vals[field];
                ele.attr('eip_value', val);
                if (ele.is(':text,select,textarea')) {
                    ele.val(val);
                    if (ele.attr('link') && toSelects.filter(ele).size() == 0)
                        ele.trigger('change.link', [true]);
                } else if (ele.is('span[eip-combox]')) eip.combox.setVal(ele, val);
                else if (ele.is(':checkbox,:radio')) {
                    ele._propAttr('checked', ele.attr('value') == val);
                }
                else {
                    var boxs = $(':checkbox[field],:radio[field]', ele);
                    if (boxs.size() == 0)
                        ele.html(val);
                    else
                        boxs.each(function () {
                            var val = vals[$(this).attr('field')] == undefined ? '' : vals[$(this).attr('field')];
                            $(this)._propAttr('checked', $(this).attr('value') == val);
                        });
                }
                var fieldSpan = $('span[eip_hint="' + ele.attr('hint') + '"]', form);
                if (fieldSpan.size() > 0) {
                    if (fieldSpan.is('[eip_display]'))
                        val = vals[fieldSpan.attr('eip_display')];
                    ele.attr('eip_display_text', val);
                    fieldSpan.text(val);
                }
            });
        },

        columnset: function (form) {
            var formid = form.attr('id').toUpperCase();
            divs = $('div[hintdiv]', form).each(function () { $(this).attr('sort', $(this).attr('osort')) });
            //var columns = page._columns.filter(function (c) { return c['FORM'] == formid });
            //for (i in columns) {
            //    var c = columns[i];
            //    divs.filter('[hintdiv="' + c['column'] + '"]').attr('sort', c['sort'] == null ? '' : c['sort']);
            //}

            divs.sort(function (a, b) {
                return parseInt(a.getAttribute('sort')) - parseInt(b.getAttribute('sort')) > 0 ? 1 : -1;
            }).appendTo(form);
            divs.filter('[sort=""]').attr('eip-hide', 'Y').hide();
            divs.filter('[sort!=""][eip-hide]').removeAttr('eip-hide').show();
            eip.hint.fit(form);
 
        },
        /*检查表单域的值
        必填：当ele已经disabled时，不检查必填项
        */
        check: function (eles,notAlert) {
            var errObj = null, err = null;
            eles.each(function () {
                var ele = $(this), val = getval(ele);
                val = nvl(val, '');
                if ($.type(val) == 'string') val = val.trim();
                if (ele.is('[必填]') && isenabled(ele) && (val == "" || !val))
                    err = "为空";
                else if (ele.is('[数值]') && val != "" && !eip.form.isNumeric(val))
                    err = "不是数值";
                else if (ele.is('[整数]') && val != "" && !eip.form.isInteger(val))
                    err = '不是整数';
                else if (ele.is('[date=Y]') && val != '' && !eip.form.isDate(val))
                    err = '不是日期';
                if (err) {
                    errObj = ele;
                    return false;
                }
            });

            if (errObj) {
                if (!notAlert) {
                    alert('"' + (errObj.attr("alert") || errObj.attr("hint") || '') + '" ' + Lang(err));
                    if (errObj.is(':text,textarea')) eip.form.focus(errObj);
                    else if (errObj.is('span[eip-combox=Y]')) eip.form.focus($(':text', errObj));
                }
                return false;
            }
            return true;

            function getval(ele) {
                if (ele.is('select,:text,textarea')) return ele.val();
                if (ele.is('span[eip-combox=Y]')) {
                    var options = ele.data('options');
                    var val = options.type == 'combox' ? $("select", ele).val() : $(':text', ele).val();
                    if ($.type(val) == 'array') val = val.length == 0 ? '' : val[0];
                    return val;
                }
                return '';
            }

            function isenabled(ele) {
                if (ele.is('span[eip-combox=Y]'))
                    return $(':text', ele).is(':enabled');
                return ele.is(':enabled');
            }
        },

        focus:function(ele){
            if (!ele.is(':text,textarea')) return;
            ele[0].setSelectionRange(0, ele.val().length);
            ele.focus();
        },

        isInteger: function (val) {
            if ($.type(val) != 'string' && $.type(val) != 'number') return false;
            return new RegExp(/^\d+$/).test(val);
        },

        isNumeric: function (val) {
            return $.isNumeric(val);
        },

        isDate: function (val) {
            if ($.type(val) != 'string') return false;
            return val.match(/^\d{4}-\d{1,2}-\d{1,2}$/) || val.match(/^\d{4}\/\d{1,2}\/\d{1,2}$/);
        }

    },

    dialog: {
        create: function (title, body, width, height, options, attrs) {
            options = $.extend({
                title: Lang(title),
                content: body,
                width: width || 600,
                height: height || 400,
                cls: "combo-p",
                closed: false,
                modal: true,
                closable:true,
                maximizable: true,
                onResize: function () {
                    eip.datagrid.fit($("table[eip-datagrid=Y]", this));
                },
                onClose: function () {
                    $(this).dialog("destroy");
                }
            }, options || {});
            return $("<div eip_dialog=Y " + (attrs || "") + "></div>").appendTo("body").dialog(options);
        },
        close: function (dlg) {
            dlg.dialog("close");
        }
    },

    /*
    在obj周围显示一个panel
    */
    panel: {
        create: function (obj, title, body, width, height, closed, options) {
            if (obj.data("panel")) return obj.data("panel").panel('close');
            //if (eip.panel.temp && eip.panel.temp.size() > 0) eip.panel.close(eip.panel.temp);
            options = $.extend({
                width: width || obj._outerWidth(),
                height: height || 200,
                doSize: false,
                closed: true,
                title: title,
                content: body,
                cls: "combo-p",
                style: { position: "absolute", zIndex: 10 }
            }, options || {});
            var panel = $("<div eip-panel=Y class=\"combo-panel\"></div>").appendTo("body");
            panel.panel(options);
            obj.data("panel", panel);
            panel.data("obj", obj);
            $(document).unbind(".combo").bind("mousedown.combo", function (e) {
                $("div[eip-panel=Y]").not($(this)).panel('close');
            });
            panel.parent().add(obj).bind("mousedown.combo", function (e) {
                obj.focus();
                return false;
            });
            if (!closed) eip.panel.open(panel);
            return panel;
        },
        open: function (panel) {

            if (eip.panel.temp) {
                eip.panel.temp.panel('close');
                eip.panel.temp = null;
            }

            var obj = panel.data("obj");
            var options = panel.panel('options');
            if (obj._outerWidth() > options.width)
                options.width = obj._outerWidth();
            panel.panel("panel").css({ zIndex: ($.fn.menu ? $.fn.menu.defaults.zIndex++ : $.fn.window.defaults.zIndex++), left: -999999 });
            panel.panel("resize", { width: options.width, height: options.height })
            var position = { left: getOffsetLeft(), top: getOffsetTop() };
            panel.panel("move", position);
            panel.panel("open");
            eip.datagrid.fit($("table[eip-datagrid=Y]", panel))
            eip.panel.temp = panel;

            function getOffsetLeft() {
                var left = obj.offset().left;
                if (panel.panel("options").width + left > window.screen.availWidth)
                    left = left - panel.panel("options").width + obj.outerWidth();
                return left;
            }

            function getOffsetTop() {
                var top = obj.offset().top + obj.outerHeight();
                if (top + panel.outerHeight() > $(document).height()
					+ $(document).scrollTop()) {
                    top = obj.offset().top - panel.outerHeight();
                }
                if (top < $(document).scrollTop()) {
                    top = obj.offset().top + obj.outerHeight();
                }
                return top;
            };
        },
        destory: function (panel) {
            panel.panel('destroy', true);
            eip.panel.temp = null;
        }
    },

    //{height:"auto",width:"auto"}
    getAttr: function (obj, attrs) {
        result = {};
        for (var key in attrs)
            result[key] = $(obj).has("[" + key + "]") > 0 ? $(obj).attr(key) : attrs[key];
        return result;
    },

    /*
    editableArray：可以编辑的电子档，默认为null，全部都可以编辑。格式：["采购-INVOICE","采购-PACKING"]，若都不可以编辑，可以传 []
    */
    upload: function (attachName, tableID, orgID, editableArray) {
        var icon = {
            other: "0px 0px",
            xls: "-32px 0px",
            xlsx: "-32px 0px",
            doc: "-64px 0px",
            ppt: "-96px 0px",
            rar: "-160px 0px",
            zip: "-192px 0px",
            htm: "-288px 0px",
            html: "-288px 0px",
            pdf: "-512px 0px",
            txt: "-672px 0px",
            jpg: "0px -32px",
            gif: "-32px -32px",
            png: "-64px -32px",
            bmp: "-96px -32px",
            tif: "-128px -32px"
        };
        var dlg = eip.dialog.create(Lang('附档'), '<div id=eip-layout><div form=Y style="position:relative"><input type=text name=tableid hint="{0}" value="{1}"></div><table id=eip-grid></table></div>'.format(Lang('单号'), tableID), 770, 500, { buttons: [{ text: Lang('关闭'), iconCls: 'icon-cancel', handler: function () { eip.dialog.close(dlg); } }] });
        eip.layout.create($('#eip-layout', dlg), [{
            obj: $('div[form=Y]', dlg), menu: false, buttons: [
                { text: '查询', icon: 'search', click: function () { list(); } }
            ]
        }]);
        $('div[form=Y]').append('<div id=p class=hint style="margin-top:3px;width:350px;display:none"></div>');
        $('<input id="eip-file" style="position:absolute;left:-1000px;" type="file">').appendTo('#eip-layout',dlg)
            .change(function () {
            var data = $(this).data('data')
            data.append('attach', this.files[0]);
            $('#p', dlg).progressbar().appendTo($('#p', dlg).parent()).show();
            $.ajax({
                url: '/common/upload.save',
                type: 'post',
                data: data,
                xhr: function () {
                    myXhr = $.ajaxSettings.xhr();
                    if (myXhr.upload) {
                        myXhr.upload.addEventListener('progress', function (e) {
                            if (e.total == 0 || e.loaded == e.total) $('#p', dlg).hide();
                            $('#p', dlg).progressbar('setValue', Math.round(e.loaded / e.total * 100));
                        }, false);
                    }
                    return myXhr;
                },
                cache: false, contentType: false, processData: false,
                success: function (data) {
                    if (eip.error(data)) return;
                    eip.datagrid.display($('#eip-grid', dlg), data);
                }
            });
        });
        eip.hint.fit($('div[form]', dlg));
        eip.datagrid.create({
            obj: $('#eip-grid', dlg),
            clear:true,
            hiddencolumns: ["PID", "ID", "TABLE_ID", "ORG", "是否成功", "权限", "文件大小"],
            columns: {
                "文件名": {
                    formatter: function (row) {
                        var size = row["文件大小"], iconCss = "";
                        if (size == null) return;
                        size == size + "&nbsp;kb";
                        iconCss = icon[row["文件名"].substr(row["文件名"].lastIndexOf(".") + 1).toLowerCase()];
                        if (!iconCss) iconCss = icon["other"];
                        var html = "<span style='display:inline-block;width:16px;height:16px;margin-right:5px;background:url(/share/images/file_ico.png) no-repeat " + iconCss + ";'>&nbsp;</span>"
                            + '<a class=blue href="/common/upload.detail?id={ID}">{文件名}</a>&nbsp;<font color=gray>({文件大小}kb)</font>'.format(row);
                        return html;
                    }
                },
                "上传人": {
                    formatter: function (row) {
                        return "<a href=# class=blue onclick=\"eip.openemployee('{上传人}')\">{上传人}</a>".format(row);
                    }
                }
            },
            dofunc: function (grid, trs) {
                var rows = eip.datagrid.getData(grid);
                for (i in trs) {
                    var tr = trs[i], row = rows[i],actionTd=$('td[field=操作]',tr);
                    if (editableArray && $.inArray(row["上传类型"], editableArray) == -1) return;
                    if (row["权限"] != "Y") return;
                    if (row["是否成功"] == "Y")
                        $('<a class=blue href=# style="margin-right:5px">删除</a>').appendTo(actionTd).click(function (e) {
                            if ($('#p', dlg).is(':visible')) return alert(Lang('正在上传，请稍后...'));
                            var row = eip.datagrid.getData(this);
                            if (!confirm(Lang('确定删除吗？\n文件：') + row['文件名'])) return;
                            eip.get('/common/upload.delete', [
                                { name: 'id', value: row['ID'] }, { name: 'tableid', value: tableID },
                                { name: 'attachname', value: attachName }, { name: 'org', value: orgID }
                            ], function (data) {
                                eip.datagrid.display($('#eip-grid', dlg), data);
                            });
                        });
                    $('<a class=blue href=# style="margin-right:5px">上传</a>').appendTo(actionTd).click(function (e) {
                        if ($('#p', dlg).is(':visible')) return alert(Lang('正在上传，请稍后...'));
                        e.stopImmediatePropagation();
                        var row = eip.datagrid.getData(this);
                        var formdata = new FormData();
                        formdata.append('tableid', tableID);
                        formdata.append('pid', row['PID']);
                        formdata.append('org', orgID || '');
                        formdata.append('attachname', attachName);
                        $('#eip-file', dlg).data('data', formdata).click().val('');
                    });
                }
            }
        });
        list();
        function list() {
            eip.datagrid.load('/common/upload.list', $('#eip-grid', dlg), [{ name: 'tableid', value: $(':text[name=tableid]', dlg).val() }, { name: 'attachname', value: attachName }, { name: 'org', value: orgID }]);
        };
    },


    //fileId 文件ID,
    //FileHost 文件地址 例如:10.88.88.15,
    //FilePath 文件路径的文件夹 例如:ElectronicDocument
    //CallBackUrl 回访地址 例如:http://10.88.88.96:800/picupload/DeletePhoto?aaa=1
    //title 打开页面的标题 例如:CLP11120931照片上传
    //uploadData 上传页面需要显示的信息 例如:装箱计划:CLP11120931 
    //uploadRemark 上传页面是否显示意见的文本域 Y 是显示
    ////打开照片上传页面
    picUploadEip: function (fileId, FileHost, FilePath, CallBackUrl, title, uploadData, uploadRemarkFlag,ServerHost) {
        var wleft = (window.screen.availWidth - 800) / 2;
        var url = "http://" + ServerHost + "/picupload/index?fileId=" + fileId + "&FileHost=" + encodeURIComponent(FileHost) + "&FilePath=" + encodeURIComponent(FilePath) + "&CallBackUrl=" + encodeURIComponent(CallBackUrl) + "&upLoadTitle=" + encodeURIComponent(title) + "&uploadData=" + encodeURIComponent(uploadData) + "&uploadRemarkFlag=" + encodeURIComponent(uploadRemarkFlag);

        eip.windowOpenObj = window.open("about:blank", "", "toolbar=0,menubar=0,status=1,left=" + wleft + ",top=30,width=800,height=630,resizable=1,scrollbars=1");
        eip.windowOpenObj.location.href = url;
        var loop = setInterval(function () {
            if (eip.windowOpenObj != null && eip.windowOpenObj.closed) {
                clearInterval(loop);
                eip.windowOpenObj = null;
                if (picUploadEipReturn != undefined && picUploadEipReturn != null) {
                    picUploadEipReturn()
                }
            }
        }, 800);

    },

    ////打开照片预览页面
    picViewEip: function (fileId, title, uploadData, uploadHtml, uploadRemark, ServerHost) {
        var uploadHtmlR = uploadHtml.replace(new RegExp(/(<)/g), '＜').replace(new RegExp(/(>)/g), '＞')
        var wleft = (window.screen.availWidth - 800) / 2;
        var url = "http://" + ServerHost + "/picview/index?fileId=" + fileId + "&upLoadTitle=" + encodeURIComponent(title) + "&uploadData=" + encodeURIComponent(uploadData) + "&uploadHtml=" + encodeURIComponent(uploadHtmlR) + "&uploadRemark=" + encodeURIComponent(uploadRemark);
        eip.windowOpenObj = window.open("about:blank", "", "toolbar=0,menubar=0,status=1,left=" + wleft + ",top=30,width=800,height=630,resizable=1,scrollbars=1");
        eip.windowOpenObj.location.href = url;
        var loop = setInterval(function () {
            if (eip.windowOpenObj != null && eip.windowOpenObj.closed) {
                clearInterval(loop);
                eip.windowOpenObj = null;
                if (picViewEipReturn != undefined && picViewEipReturn != null) {
                    picViewEipReturn()
                }
            }
        }, 800);


    },


    imports: function (options) {
        var options = $.extend({
            title: Lang("数据导入"),
            url: document.URL.replace(/(.*\/){0,}([^\.]+).*/ig, "$2").replace('#', '') + ".imports",
            content: "需copy栏位如下，其中黄色背景为必需要导的栏位。",    //导入对话框的说明信息
            columns: {},
            /*
            columns: {
            "工单号": {
            dataType: "string",
            allowNull: true, //如果该列有导入，则该栏位的值是否必填
            optional: false //该列是否可选
            }*/
            toolbarMore: null,
            width: 800,
            height: 500,
            close: true,   //点击确定导入后,是否关闭窗口 
            submit: null,   //点击确定导入时，要执行的函数，若该函数返回false，则不会继续导入。调用该函数时，会传递导入对话框的jquery对象
            doFunc: null,
            para: null       //补充参数
        }, options || {});;
        var contentStr = options.content == "" ? "" : "<div style='padding:2px'>" + options.content + "</div>";
        var textInfo = Lang("从excel复制内容到此(包括标题行)，点击按钮“读取”,检查并编辑下表，确认无误后点击“确定导入”");
        var fieldsStr = "<table id=eip_import_fields_tb border=1 style='border-collapse: collapse;margin:3px;'  cellpadding=3 bgcolor='#99CCFF' bordercolorlight='#000099' bordercolordark='#003399'><tr>";
        var eip_columns = [];
        for (var i in options.columns) {
            var col = options.columns[i];
            fieldsStr += "<td field=\"" + col['column'] + "\" title='" + (col.optional ? Lang("可选") + "'" : Lang("必填") + "' bgcolor=yellow") + ">" + Lang(col['column']) + "</td>";
        }
        fieldsStr += "</tr></table>";

        var importDialog = eip.dialog.create("&nbsp;" + options.title, "<div id=imports-layout><div imports-form style='padding:10px;background:#eee' region='north'></div><table id=imports-grid></table></div>", options.width, options.height, { iconCls: "icon-imports" });
        var form = $('div[imports-form]', importDialog);
        form.append('<div>' + options.content + '</div>').append(fieldsStr);
        $("<textarea name=eip_import_textarea placeholder='" + textInfo + "'></textarea>").appendTo(form).css({ height: 50, width: form.width() });
        var grid = $('#imports-grid', importDialog);
        eip.datagrid.create({
            obj: grid,
            lockcolumns: 0,
            shownum: false,
            pagesize: 9999,
            dofunc: function (grid) {
                var colattr = {}, format = { number: ' 数值', datetime: ' date=Y'};
                for (var i in options.columns) {
                    var col = options.columns[i], str = '';
                    if (format[col.type]) str += format[col.type];
                    if (!col.empty) str += " 必填";
                    colattr[col.column] = str;
                }
                $("td[field]", grid.data('options').dc.body2).each(function () {
                    var td = $(this);
                    td.html('<input name="{0}" type=text value="{1}" {2} alert="{3}">'.format(td.attr('field'), td.text(), colattr[td.attr('field') || ''], td.attr('field')));
                    $(':text', td)._outerWidth(td.width());
                });
            }
        });
        eip.layout.create($("#imports-layout", importDialog), {
            obj: form,
            buttons: [{
                text: '读取', icon: 'preview', click: function () {
                    var charEnter = String.fromCharCode(1), charTab = String.fromCharCode(2);
                    var regEnter = new RegExp(charEnter, "g");
                    var regTab = new RegExp(charTab, "g");
                    var encodeStr = function (str) {
                        str = str.replace(/"/g, "&quot;");
                        var re = /"[^"]+?"/g;
                        var r = str.match(re);
                        if (r)
                            for (var i = 0; i < r.length; i++)
                                str = str.replace(r[i], r[i].replace(/"/g, "").replace(/\n/g, charEnter).replace(/\t/g, charTab));
                        return str;
                    };

                    var text = $("textarea[name=eip_import_textarea]", form).val().trim();
                    if (text == "") return alert(Lang("请从excel复制内容到文本框"));
                    text = encodeStr(text);
                    var data = text.split("\n");
                    var cols = data[0].trim().split(String.fromCharCode(9));

                    var noCols = options.columns.filter(function (c) {
                        return !c.optional && $.inArray(c.column, cols) == -1;
                    });
                    if (noCols.length > 0)
                        return alert('"' + noCols.map(function (c) { return c.column }).join('", "') + '" 为必须导入栏位');

                    cols = cols.map(function (cstr) {
                        var col = options.columns.filter(function (c) { return c.column == cstr; });
                        if (col.length > 0) {
                            col[0].sort = 1;
                            return col[0];
                        } else
                            return {};
                    });                    
                    var rows = data.slice(1).map(function (str) {
                        var rowa = str.split(String.fromCharCode(9)), row = {};
                        for (var i in rowa)
                            row[cols[i].column] = rowa[i];
                        return row;
                    });
                    cols = cols.filter(function (c) { return c.column });
                    data = { columns: cols, rows: rows, total: rows.length, page: 1 };
                    eip.datagrid.display(grid, data);
                }
            }, {
                text: '确定导入', icon: 'imports', click: function () {
                    if (!eip.datagrid.getData(grid)) return alert(Lang('没有内容可导入。'));
                    var eles = $(':input[name],select[name]', importDialog);
                    if (!eip.form.check(eles)) return;
                  
                    if (options.submit && !options.submit(importDialog)) return;
                    
                    eip.post(options.url, eip.para.create(eles), function (val) {
                       
                        options.doFunc && options.doFunc(val);
                        if (options.close) eip.dialog.close(importDialog);
                    }, true);
                }
            }].concat(options.toolbarMore || [])
        });
        eip.hint.fit(form);
        return importDialog;
    },

    addCommas: function (number) {
        if (number == null || number == undefined) return '';
        var nStr = number + '';
        x = nStr.split('.');
        x1 = x[0];
        x2 = x.length > 1 ? '.' + x[1] : '';
        var rgx = /(\d+)(\d{3})/;
        while (rgx.test(x1)) {
            x1 = x1.replace(rgx, '$1' + ',' + '$2');
        }
        return x1 + x2;
    },

    editor: {
        create: function (obj, html,dofunc) {
            eip.editor._id = eip.editor._id ? eip.editor._id + 1 : 1;
            eip.loadScript('/share/includes/ckeditor/ckeditor.js', function () {
                CKEDITOR.editorConfig = function (config) {
                    config.toolbarGroups = [
                        { name: 'document', groups: ['mode', 'document', 'doctools'] },
                        { name: 'editing', groups: ['find', 'selection', 'spellchecker', 'editing'] },
                        { name: 'forms', groups: ['forms'] },
                        { name: 'basicstyles', groups: ['basicstyles', 'cleanup'] },
                        { name: 'paragraph', groups: ['list', 'indent', 'blocks', 'align', 'bidi', 'paragraph'] },
                        { name: 'links', groups: ['links'] },
                        { name: 'clipboard', groups: ['clipboard', 'undo'] },
                        { name: 'insert', groups: ['insert'] },
                        { name: 'styles', groups: ['styles'] },
                        { name: 'colors', groups: ['colors'] },
                        { name: 'tools', groups: ['tools'] },
                        { name: 'others', groups: ['others'] },
                        { name: 'about', groups: ['about'] }
                    ];

                    config.removeButtons = 'Cut,Copy,Paste,Anchor,Underline,Strike,Subscript,Superscript,About';
                };
                $('<textarea style="display:none" id=eip-editor{0}></textarea>'.format(eip.editor._id)).appendTo(obj.empty()).val(html);
                CKEDITOR.config.height = obj.height() - 44;
                var editor = CKEDITOR.replace('eip-editor' + eip.editor._id);
                obj.data('eip-editor', editor).attr('eip-editor', 'Y');
                var pbody = obj.closest('div.panel-body');
                pbody.data('eip-resize').push(function () {
                    eip.editor.fit(obj);
                });
                dofunc && dofunc(editor);
            });
        },
        getdata: function (obj) {
            return obj.data('eip-editor').getData();
        },
        fit: function (obj) {
            obj.data('eip-editor').resize('100%', obj.height() - 44, true);
        }
    },

    loadScript: function (url, callback) {
        if ($("[src*='" + url + "']").size() > 0) return callback && callback();
        var head = document.getElementsByTagName('head')[0];
        var script = document.createElement('script');
        script.type = 'text/javascript';
        script.src = url;
        script.onload = script.onreadystatechange = function () {
            if ((!this.readyState || this.readyState === "loaded" || this.readyState === "complete")) {
                callback && callback();
                script.onload = script.onreadystatechange = null;
            }
        };
        head.appendChild(script);
    },

    /*显示eip意见输入框
    obj：点击意见的对象，user输入意见后，会执行obj.html(remark)。
    table_name：对应意见表的remark_name，用以区别各种意见。一般设为你的grid对应的主表名
    table_id：你grid里的唯一id
    notes_title：如果user选择发邮件，此为邮件的默认标题
    notes_link：user的邮件里，默认会添加一个超级链接，方便收到意见的人，点击链接，回到EIP的对应报表
    */
    remark: function (obj, table_name, table_id, notes_title, notes_link) {
        var dlg = eip.dialog.create('意见', '<div id=layout><div form=Y style="display:none" right=310><input type=text name=mailtitle hint="主旨" value="{0}"><span id=mailto hint="发送给"></span></div><div id=editordiv region=center></div><div region="south" data-options="split:true" height=250><table id=eip_remark_gd></table></div></div>'.format(notes_title), 800, 500);
        var para = [
            { name: 'table_name', value: table_name },
            { name: 'table_id', value: table_id },
            { name: 'notes_link', value: notes_link }
        ];
        eip.combox.create({
            obj: $('#mailto', dlg),
            url: '/common/comboxs.mail',
            type: 'search',
            multiple:true
        });

        eip.datagrid.create({
            lockcolumns:0,
            obj: $('#eip_remark_gd', dlg),
            wrap: true,
            columns: {
                意见: { formatter: function (row) { return row['意见'].replace(/(<([^>]+)>)/ig, '');} }
            },
            dofunc: function (grid) {
                $('tr[seq]', grid).click(function () {
                    var row = eip.datagrid.getData(this);
                    var html = '{意见}<br><font color=gray>[{发布人}&nbsp;{发布日期}]</font>'.format(row);
                    editor.editable().setHtml(html);
                });
            }
        });
        eip.datagrid.load('/common/remark.list', $('#eip_remark_gd', dlg), para);

        eip.layout.create($('#layout', dlg), {
            obj: $('div[form=Y]', dlg), buttons: [
                { html: '<span><input id=send_notes_flag type=checkbox>发送邮件</span>' },
                {
                    text: '发送', icon: 'email', click: function () {
                        var remark = eip.editor.getdata($('#editordiv', dlg)).trim();
                        if (remark == '') return alert(Lang('请输入意见'));                        
                        if ($('#send_notes_flag', dlg).is(':checked')) {
                            var mailto = $('[name=mailto]', dlg).val().trim();
                            if (mailto == '') return alert(Lang('请填写“发送给”'));
                            para = para.concat([
                                { name: 'mailtitle', value: $('[name=mailtitle]', dlg).val() },
                                { name: 'mailto', value: mailto }
                            ]);
                        }
                        para.push({ name: 'remark', value: remark });
                        eip.post('/common/remark.save', para, function (data) {
                            if (data != 'Y') return alert(Lang(data));
                            eip.tojq(obj).html(remark.replace(/(<([^>]+)>)/ig, ''));
                            eip.dialog.close(dlg);
                        }, this);
                    }
                }]
        });
        $('#send_notes_flag', dlg).click(function () {
            var form = $('div[form=Y]', dlg);
            if ($(this).is(':checked')) form.show();
            else form.hide();
            eip.hint.fit(form);
        });


        $('#attachBtn', dlg).click(function () {
            eip.hint.add($('div[form=Y]', dlg), $("<input hint='' type=file>"));
            eip.hint.fit($('div[form=Y]', dlg));
        });
        var editor = null;
        eip.editor.create($('#editordiv', dlg), '', function (edt) { editor = edt });
    },

    /*鼠标停在对象上显示提示框
    pop：要显示的内容，可以是html
    popurl：发送ajax，从后台获取内容
    pop和popurl可以定义在obj的属性上，调用pop函数时不传递这2个参数
    */
    pop: function (obj, pop, popurl) {
        var dataObj = this == eip ? null : this;
        obj = eip.tojq(obj);
        if (!pop && !popurl) {
            if (obj.is('[pop]')) {
                eval('pop=' + obj.attr('pop'));
                if ($.type(pop) == 'function') pop = pop(dataObj || obj);
            } else if (obj.is('[popurl]')) {
                eval('popurl=' + obj.attr('popurl'));
                if ($.type(popurl) == 'function') popurl = popurl(dataObj || obj);
            }
            if (!pop && !popurl) return;
        }

        var options = {
            content: pop,
            hideEvent: 'none',
            onShow: function () {
                var t = $(this);
                $('a.easyui-linkbutton', t.tooltip('tip')).linkbutton();
                t.tooltip('tip').focus().unbind().bind('blur mouseleave.eip', function () {
                    t.tooltip('hide');                    
                    t.removeAttr('eip-pop-hiding');
                }).unbind('mouseover.eip').bind('mouseover.eip', function () {
                    t.removeAttr('eip-pop-hiding');
                });
            }
        };

        if (popurl) {
            options.content = $('<div></div>');
            options.onShow = function () {
                $(this).tooltip('arrow').css('left', 20);
                $(this).tooltip('tip').css('left', $(this).offset().left);
            };
            options.onUpdate = function (cc) {
                if (popurl.substr(0, 1) == '.') popurl = eip.url + popurl;
                cc.panel({
                    width: 300,
                    height: 'auto',
                    border: false,
                    href: popurl
                });
            };
        }

        obj.tooltip(options);
        obj.unbind('mouseout.eip').bind('mouseout.eip', function () {
            obj.attr('eip-pop-hiding','Y');
            window.setTimeout(function (obj) {
                if (obj.attr('eip-pop-hiding') == 'Y')
                    obj.tooltip('hide');
            }, 200, $(this));
        });
    },

    tojq: function (obj) {
        return (obj instanceof jQuery) ? obj : $(obj);
    },
    server: 'http://172.16.25.51'

}
 
function AjaxError(ajaxRequest, textStatus, errorThrown) {
    switch (ajaxRequest.status) {
        case 401:
            //if (parent.Login)
            //    parent.Login(false);
            //else
                alert(Lang("登录已过期或您没有权限。"));
            eip.login();
            break;
        default:
            if (textStatus != "abort")
                alert(Lang("对不起，请求“{0}”出错，请联系IT!\n\n错误信息：{1}".format(this.url, textStatus)));
            break;
    }
}

function nvl(val, newVal) {
    if (val == undefined || val == null || (val instanceof jQuery && val.size() < 1)) return newVal;
    return val;
}

function Log(val) {
    console.log(val);
}

function Lang(key) {
    if (!key) return "";
    var args = arguments;
    //var val = _eip_lang["SIMPLIFIED"][key] || _eip_lang_p[key];
    var val = key;
    if (args.length > 1)
        val = val.replace(/\{(\d+)\}/g,
        function (m, i) {
            return args[parseInt(i) + 1];
        });
    return val;
}

/*支持 '{0}'.format('abc')
       '{文件}'.format({'文件':'abc'}) */
String.prototype.format = function () {
    var args = arguments;
    if (args.length == 1 && $.type(args[0]) == 'object')
        return this.lang(args[0]);
    else if (args.length == 1 && $.type(args[0]) == 'array')
        args = args[0];

    return this.replace(/\{(\d+)\}/g,
        function (m, i) {
            var val = args[i];
            return val == null || val == undefined ? '' : val;
        });
}

String.prototype.trim = function () {
    return this.replace(/(^\s*)|(\s*$)/g, "");
}

//dict为空，就是翻译，否则就按dict处理
String.prototype.lang = function (dict) {    
    return this.replace(/\{([^\}]+)\}/g,
        function (m, key) {
            //if (dict) var val = dict[key];
            //else val = _eip_lang[_lang_user][key] || _eip_lang_p[key];
            //if (val == null || val == undefined) val = '';
            //if (val == '' && !dict) val = key;
            //return val;
            return key;
        });
}

Date.prototype.addDays = function (d) {
    this.setDate(this.getDate() + d);
    return this;
};

Date.prototype.addMonths = function (m) {
    this.setMonth(this.getMonth() + m);
    return this;
};


Date.prototype.format = function (fmt) {
    // author: meizz  
    var o = {
        "M+": this.getMonth() + 1, // 月份  
        "d+": this.getDate(), // 日  
        "h+": this.getHours(), // 小时  
        "m+": this.getMinutes(), // 分  
        "s+": this.getSeconds(), // 秒  
        "q+": Math.floor((this.getMonth() + 3) / 3), // 季度  
        "S": this.getMilliseconds()
        // 毫秒  
    };
    if (/(y+)/.test(fmt))
        fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4
                - RegExp.$1.length));
    for (var k in o)
        if (new RegExp("(" + k + ")").test(fmt))
            fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1)
                    ? (o[k])
                    : (("00" + o[k]).substr(("" + o[k]).length)));
    return fmt;
}

$(function () {
    $("#eip-mask-msg").html(Lang("系统正在处理中，请稍候") + "...");
    var url = window.location.href.replace('//', '').replace('#', '');
    url = url.substr(url.indexOf('/'));
    eip.url = url.split('?')[0];
    eip._eip_message = {};
    //window.addEventListener("message", function (e) {
    //    var msg = e.data;
    //    if (eip._message && eip._message[msg.id]) {
    //        eip._message[msg.id](msg.data);
    //        delete eip._message[msg.id];
    //        return;
    //    }
    //    switch (msg.action) {
    //        case "reload":
    //            window.location.reload();
    //            break;
    //        default:
    //            if (eip._eip_message[msg.action])
    //                eip._eip_message[msg.action](msg.data);
    //            break;
    //    }
    //});    
    eip.postmessage('getini', null, null,null, function (data) {
        eip._ini = data;
        $('select[ini]').each(function () {
            iniVal = eip._ini[$(this).attr('ini')] || '';
            if ($(this).val() == '' && iniVal != '') $(this).val(iniVal);
            if (iniVal != '' && $(this).is('select[link]')) {
                $(this).trigger('change.link');
            }
        });
    });

    $.ajaxSetup({
        error: function (ajaxRequest, textStatus, errorThrown) {
            AjaxError.apply(this, arguments);
            eip.disabled(eip['_disabled'], false);            
            eip.showloading(true);
        }
    });

});

/*
重新定义setTimeout与setInterval
不能使用 setTimeout('a()',1000)
要改为 setTimeout(a,1000)
传参：setTimeout(a,1000,val1,val2);
*/
if (! +[1, ]) {
    (function (f) {
        window.setTimeout = f(window.setTimeout);
        window.setInterval = f(window.setInterval);
    })(function (f) {
        return function (c, t) {
            var a = [].slice.call(arguments, 2);
            return f(function () {
                c.apply(this, a)
            }, t)
        }
    });
}



$.extend($.fn.validatebox.defaults.rules, {
    idcard: {// 验证身份证
        validator: function (value) {
            return /^\d{15}(\d{2}[A-Za-z0-9])?$/i.test(value);
        },
        message: '身份证号码格式不正确'
    },
    minLength: {
        validator: function (value, param) {
            return value.length >= param[0];
        },
        message: '请输入至少（2）个字符.'
    },
    length: {
        validator: function (value, param) {
            var len = $.trim(value).length;
            return len >= param[0] && len <= param[1];
        },
        message: "输入内容长度必须介于{0}和{1}之间."
    },
    phone: {// 验证电话号码
        validator: function (value) {
            return /^((\d2,3 )|(\d{3}\-))?(0\d2,3 |0\d{2,3}-)?[1-9]\d{6,7}(\-\d{1,4})?$/i.test(value);
        },
        message: '格式不正确,请使用下面格式:020-88888888'
    },
    mobile: {// 验证手机号码
        validator: function (value) {
            return /^(13|15|18)\d{9}$/i.test(value);
        },
        message: '手机号码格式不正确'
    },
    number: {// 验证整数或小数
        validator: function (value) {
            return /^\d+(\.\d+)?$/i.test(value);
        },
        message: '请输入数字，并确保格式正确'
    },
    currency: {// 验证货币
        validator: function (value) {
            return /^\d+(\.\d+)?$/i.test(value);
        },
        message: '货币格式不正确'
    },
    qq: {// 验证QQ,从10000开始
        validator: function (value) {
            return /^[1-9]\d{4,9}$/i.test(value);
        },
        message: 'QQ号码格式不正确'
    },
    integer: {// 验证整数 
        validator: function (value) {
            return /^[+]?[1-9]+\d*$/i.test(value);
            //可正负数
            //return /^([+]?[0-9])|([-]?[0-9])+\d*$/i.test(value);
        },
        message: '请输入整数'
    },
    age: {// 验证年龄
        validator: function (value) {
            return /^(?:[1-9][0-9]?|1[01][0-9]|120)$/i.test(value);
        },
        message: '年龄必须是0到120之间的整数'
    },

    chinese: {// 验证中文
        validator: function (value) {
            return /^[\Α-\￥]+$/i.test(value);
        },
        message: '请输入中文'
    },
    english: {// 验证英语
        validator: function (value) {
            return /^[A-Za-z]+$/i.test(value);
        },
        message: '请输入英文'
    },
    unnormal: {// 验证是否包含空格和非法字符
        validator: function (value) {
            return /.+/i.test(value);
        },
        message: '输入值不能为空和包含其他非法字符'
    },
    username: {// 验证用户名
        validator: function (value) {
            return /^[a-zA-Z][a-zA-Z0-9_]{5,15}$/i.test(value);
        },
        message: '用户名不合法（字母开头，允许6-16字节，允许字母数字下划线）'
    },
    faxno: {// 验证传真
        validator: function (value) {
            //            return /^[+]{0,1}(\d){1,3}[ ]?([-]?((\d)|[ ]){1,12})+$/i.test(value);
            return /^((\d2,3 )|(\d{3}\-))?(0\d2,3 |0\d{2,3}-)?[1-9]\d{6,7}(\-\d{1,4})?$/i.test(value);
        },
        message: '传真号码不正确'
    },
    zip: {// 验证邮政编码
        validator: function (value) {
            return /^[1-9]\d{5}$/i.test(value);
        },
        message: '邮政编码格式不正确'
    },
    ip: {// 验证IP地址
        validator: function (value) {
            return /d+.d+.d+.d+/i.test(value);
        },
        message: 'IP地址格式不正确'
    },
    name: {// 验证姓名，可以是中文或英文
        validator: function (value) {
            return /^[\Α-\￥]+$/i.test(value) | /^\w+[\w\s]+\w+$/i.test(value);
        },
        message: '请输入姓名'
    },
    date: {// 验证姓名，可以是中文或英文
        validator: function (value) {
            //格式yyyy-MM-dd或yyyy-M-d
            return /^(?:(?!0000)[0-9]{4}([-]?)(?:(?:0?[1-9]|1[0-2])\1(?:0?[1-9]|1[0-9]|2[0-8])|(?:0?[13-9]|1[0-2])\1(?:29|30)|(?:0?[13578]|1[02])\1(?:31))|(?:[0-9]{2}(?:0[48]|[2468][048]|[13579][26])|(?:0[48]|[2468][048]|[13579][26])00)([-]?)0?2\2(?:29))$/i.test(value);
        },
        message: '请输入合适的日期格式'
    },
    datetime: { 
        validator: function (value) {
            //格式yyyy-MM-dd或yyyy-M-d
            return /^\d{4}[-]([0][1-9]|(1[0-2]))[-]([1-9]|([012]\d)|(3[01]))([ \t\n\x0B\f\r])(([0-1]{1}[0-9]{1})|([2]{1}[0-4]{1}))([:])(([0-5]{1}[0-9]{1}|[6]{1}[0]{1}))([:])((([0-5]{1}[0-9]{1}|[6]{1}[0]{1})))$/i.test(value);
        },
        message: '请输入合适的日期格式'
    },
    msn: {
        validator: function (value) {
            return /^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/.test(value);
        },
        message: '请输入有效的msn账号(例：abc@hotnail(msn/live).com)'
    },
    same: {
        validator: function (value, param) {
            if ($("#" + param[0]).val() != "" && value != "") {
                return $("#" + param[0]).val() == value;
            } else {
                return true;
            }
        },
        message: '两次输入的密码不一致！'
    },
    regex: {
        validator: function (value, param) {
            //转义C#正则表达式,将×变成\\然后做校验
            eval("var re=/" + param[0].replace(/×/g, "\\") + "/")
            return re.test(value)
        },
        message: '数据格式不符合！'

    }
});
 

(function ($) {
    $.extend($,
        {
            eipMsg: {
                alert: function (alertInfo, dofunc) {
                    $.messager.alert("系统提示", alertInfo, "info", function () {
                            if (dofunc) dofunc();
                        }
                    );
                },
                show: function (showInfo, dofunc, timeout) {
                    if (!timeout) timeout = 300  
                    var aa=$.messager.show({
                        title: "操作提示",
                        msg: "<div style=\"text-align:center;\">"+showInfo+"</div>",
                        showType: "fade",
                        width: 180,
                        height: 80,
                        timeout: timeout,
                        style: {
                            top: document.body.clientHeight / 2-50,
                            left: document.body.clientWidth / 2-80,
                            bottom: '',
                            display: 'none'
                        }
                    });
                    if (dofunc) dofunc();
                },
                confirm: function (confirmInfo, dofunc) {
                    $.messager.confirm("操作提示", confirmInfo, function (data) {
                        if (data && dofunc) {
                             dofunc();
                        } 
                    });
                   // $(".panel-tool-close").css("display", "none");
                },
                prompt: function (promptInfo, dofunc) {
                    $.messager.prompt("操作提示", promptInfo, function (data) {
                        if (data && dofunc) {
                            dofunc();
                        }
                    });
                   // $(".panel-tool-close").css("display", "none");
                }

            },
            //统一处理 返回的json数据格式
            procAjaxData: function (data, sucFunc, errFunc) {
                if (!data || !data.Statu) {
                    return;
                }
                
                switch (data.Statu) {
                    case "ok":
                        if (data.Msg && data.Msg.trim() != "") {
                            if (sucFunc())
                                $.eipMsg.show(data.Msg, sucFunc(data));
                            else
                                $.eipMsg.show(data.Msg);
                        }
                        else {
                            if (sucFunc) sucFunc(data);
                        }
                        break;
                    case "err":
                        if (data.Msg && data.Msg.trim() != "") {
                            if (errFunc)
                                $.eipMsg.alert(data.Msg, errFunc(data));
                            else
                                $.eipMsg.alert(data.Msg);
                        }
                        else {
                            if (errFunc) errFunc(data);
                        }
                        
                        break;
                    case "nologin":
                        $.eipMsg.alert(data.Msg);
                        break;
                }
                $.redirect(data);
            },
            redirect: function (data) {
                if (data.BackUrl) {
                    if (window.top)
                        window.top.location = data.BackUrl;
                    else
                        window.location = data.BackUrl;
                }

            }
        });
}(jQuery));
