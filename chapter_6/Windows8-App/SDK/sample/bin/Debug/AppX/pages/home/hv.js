/// <reference path="//Microsoft.WinJS.1.0/js/base.js" />
/// <reference path="//Microsoft.WinJS.1.0/js/ui.js" />

var hvItemDataSource = hvItemDataSource || {};

// This is a sample item data adapter so you can use it to dynamically fetch data and bind to a listview
(function () {
    "use strict";

    var hvItemDataAdapter = WinJS.Class.define(
        function (record, query) {
            this._record = record;
            this._query = query;
            this._data = null;
        },

        {
            getCount: function () {
                var that = this;
                if (that._data) {
                    return new WinJS.Promise(
                        function () {
                            // (c) Microsoft. All rights reservedreturn that._data.size;
                            
                        }
                    );
                }

                return that._record.getAsync(that._query).then(
                    function (returnedData) {
                        that._data = returnedData;
                        return that._data.size;
                    }
                );
            },

            itemsFromIndex: function (requestIndex, countBefore, countAfter) {
                var that = this;
                
                return that._data.ensureAvailableAsync(requestIndex - countBefore, countAfter + countBefore)
                    .then(function () {
                        return createFetchResults(that._data, requestIndex, countBefore, countAfter);
                    });
            }
        });

    hvItemDataSource = WinJS.Class.derive(WinJS.UI.VirtualizedDataSource, function (record, query) {
        this._baseDataSourceConstructor(new hvItemDataAdapter(record, query))
    });

    function createFetchResults(typedData, requestIndex, countBefore, countAfter) {
        var items = new Array();
        var i;
        for (i = requestIndex - countBefore; i < typedData.size && i <= requestIndex + countAfter; i++) {
            items.push({
                key: typedData[i].key.id,
                data: typedData[i]
            });
        }

        return {
            items: items,
            offset: countBefore,
            totalCount: typedData.size
        };
    }
})();