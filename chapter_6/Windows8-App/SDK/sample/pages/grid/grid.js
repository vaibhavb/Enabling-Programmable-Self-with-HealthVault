// (c) Microsoft. All rights reserved
/// <reference path="/pages/home/test.js" />

(function () {
    "use strict";

    WinJS.UI.Pages.define("/pages/grid/grid.html", {
        // This function is called whenever a user navigates to this page. It
        // populates the page elements with the app's data.
        ready: function (element, options) {
            loadGrid();
            
            var dataTransferManager = Windows.ApplicationModel.DataTransfer.DataTransferManager.getForCurrentView();
            dataTransferManager.addEventListener("datarequested", dataRequested);
        },
        
        unload: function () {
            var dataTransferManager = Windows.ApplicationModel.DataTransfer.DataTransferManager.getForCurrentView();
            dataTransferManager.removeEventListener("datarequested", dataRequested);
        }
    });
    
    function dataRequested(e) {
        var request = e.request;

        request.data.properties.title = "HealthVault Sample App";
        request.data.properties.description = "Sharing some weights";

        request.data.setDataProvider("HealthVault.ItemTypes." + HealthVault.ItemTypes.Weight.typeID, dataShareItems);
        request.data.setDataProvider("HealthVault.Types.RecordItem", dataShareItems);
    }
    
    function dataShareItems(request) {
        var deferral = request.getDeferral();
        
        var record = getCurrentRecord();
        var query = HealthVault.ItemTypes.Weight.queryFor();

        record.getAllItemsAsync(query).then(function (items) {
            request.setData(HealthVault.Types.RecordItem.serializeMultiple(items));
            deferral.complete();
        },
        function () {
            deferral.complete();
        }
        );
    }

    function getDataSource() {
        var record = getCurrentRecord();
        var query = HealthVault.ItemTypes.Weight.queryFor();

        var dataSource = new hvItemDataSource(record, query);
        return dataSource;
    }

    function loadGrid() {
        var list = document.getElementById("listview1").winControl;
        var dataSource = getDataSource();

        list.itemDataSource = dataSource;
        list.itemTemplate = templateFunction;
    }

    function templateFunction(itemPromise) {
        return itemPromise.then(function (item) {
            var template = document.getElementById("itemTemplate");
            if (item.data.value.value.value < 60) {
                template = document.getElementById("itemAltTemplate");
            }

            var container = document.createElement("div");
            // We could pass in a view model here to make the binding of strings easier.
            template.winControl.render(item.data, container);
            return container;
        });
    }
})();
