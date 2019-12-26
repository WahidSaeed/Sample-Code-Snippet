/*
* Dynamic Form Builder 
* Created by : Abdul Wahid
*/

function DynamicFormBuilder(TempID, GridHolderID) {

    this.TempID = TempID;
    this.GridHolderID = GridHolderID;
    this.ForTree = false;
    this.isAdd = false;
    this.isEdit = false;
    this.isDelete = false;
    this.addBtnText = "Add New Record";
    this.ModalTitle = "Modal Title";
    this.ModalWidth = "";
    this.GridFilter = "";
    this.OnRowClickRedirectLink;
    this.JsonForHideShow = null;
    this.DynamicRowOnClick = function (FormID) {
        if (this.OnRowClickRedirectLink) {
            this.OnRowClickRedirectLink(FormID);
        }
    };
    this.onRowClick = function (func) {
        if (func) {
            this.OnRowClickRedirectLink = function (FormID) { func(FormID) };
        }
    };
    this.afterFormBuilt = function (formID) {

    }
    this.CustomValidation = function (valid) {
        return valid;
    };
    this.IsValidDynamicFields = function () {

        var valid = true;

        $('[valid-ajax]').each(function () {

            if ($(this).attr('columntype') == 'TextEditor') {
                if (nicEditors.findEditor(this.id).getContent() == "" || nicEditors.findEditor(this.id).getContent() == "<br>") {
                    valid = false;
                    $('#' + $(this).attr('valid-ajax')).show();
                }
                else {
                    $('#' + $(this).attr('valid-ajax')).hide();
                }
            }
            else {

                if ($(this).val() == "" || $(this).val() == "º") {
                    valid = false;
                    $('#' + $(this).attr('valid-ajax')).show();
                }
                else {
                    $('#' + $(this).attr('valid-ajax')).hide();
                }
            }
        });

        if (!valid) {
            alert("Please fill mandatory field(s).");
        }

        if (valid) {
            valid = this.CustomValidation(valid);
        }

        return valid;
    };
    this.JsonHideShowFields = function (JSON) {

        for (var i = 0; i < JSON.length; i++) {
            var Input = $('#' + JSON[i].FieldName);

            if (JSON[i].Value == Input.val()) {
                $('#td' + JSON[i].FieldToHide).hide();
            }
            else {
                $('#td' + JSON[i].FieldToHide).show();
            }
        }

    };
    this.SetEventOnFields = function () {
        var Obj = this;
        $('[columntype]').each(function () {
            $(this).change(function () {
                Obj.JsonHideShowFields(Obj.JsonForHideShow);
            });
        });
    };
    this.SetQueryStringOnHidden = function () {
        var getParameterByName = function (name, url) {
            if (!url) url = window.location.href;
            name = name.replace(/[\[\]]/g, "\\$&");
            var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
        results = regex.exec(url);
            if (!results) return null;
            if (!results[2]) return '';
            return decodeURIComponent(results[2].replace(/\+/g, " "));
        };
        $("[columntype='Hidden'][QString]").each(function () {
            $(this).val(getParameterByName($(this).val()));
        });
    };

    var AJAX = new AJAXsupport();
    AJAX.resetVar();

    AJAX.addData("TempID", this.TempID);
    AJAX.addData("SaveType", 'getBasicProp');

    var Obj = this;
    var sucSave = function () {

        if (AJAX.getData('ForTree')) {
            Obj.ForTree = JSON.parse(AJAX.getData('ForTree'));
        }
        if (AJAX.getData('isAdd')) {
            Obj.isAdd = JSON.parse(AJAX.getData('isAdd'));
        }
        if (AJAX.getData('isEdit')) {
            Obj.isEdit = JSON.parse(AJAX.getData('isEdit'));
        }
        if (AJAX.getData('isDelete')) {
            Obj.isDelete = JSON.parse(AJAX.getData('isDelete'));
        }
        Obj.addBtnText = AJAX.getData('AddbtnText');
        Obj.ModalTitle = AJAX.getData('TableName');
        Obj.ModalWidth = AJAX.getData('ModalWidth');

        $('Body').prepend('<div id="DynamicModal' + Obj.TempID + '" class="modal fade in modal-overflow" tabindex="-1" data-width="' + Obj.ModalWidth + '" aria-hidden="true"> <div class="modal-header"><h4 class="modal-title" lang="' + Obj.ModalTitle.replace(/ /g, "_") + '">' + Obj.ModalTitle + '</h4></div><div class="modal-body"><div id="dvDynamic' + Obj.TempID + '"></div></div></div>');
    }

    DynamicSave(AJAX, sucSave, false);
}

DynamicFormBuilder.prototype.showDynamicGrid = function () {


    if (!this.ForTree) {
        var AJAX = new AJAXsupport();
        AJAX.resetVar();

        AJAX.addData("TempID", this.TempID);
        AJAX.addData("SaveType", 'GetDynamicGridHeader');

        var Obj = this;
        var sucSave = function () {

            var getUrl = window.location;
            var baseUrl = getUrl.protocol + "//" + getUrl.host + (getUrl.pathname.split('/')[1] == 'UILayer' ? '' : "/" + getUrl.pathname.split('/')[1]);

            $('#' + Obj.GridHolderID).html(AJAX.getRefreshText());
            $('#' + Obj.GridHolderID + ' .tbldata').dataTable({
                "bDestroy": true,
                "bServerSide": true,
                "bProcessing": true,
                "bJQueryUI": false,
                "bPaginate": true,
                "sAjaxSource": baseUrl + '/UILayer/Compliance/AJAX/AjaxGridList.aspx?type=DynamicGrid$' + Obj.TempID + '$' + Obj.GridFilter,
                "sPaginationType": "full_numbers",
                "bAutoWidth": false,
                "fnDrawCallback": function () {
                    if (Obj.isAdd) {
                        if ($("#AddBtn" + Obj.TempID).length == 0) {
                            $('<input type="button" class="btn btn-default" style="margin: 0 10px 10px 0;" id="AddBtn' + Obj.TempID + '" value="' + Obj.addBtnText + '" />').prependTo('div.dataTables_filter');
                            $("#AddBtn" + Obj.TempID).click(function () {
                                Obj.showViewInModal();
                            });
                        }
                    }
                    $('[ClickEdit' + Obj.TempID + '], [ClickDelete' + Obj.TempID + ']').each(function () {
                        var ID = $(this).attr('ClickID');
                        $(this).removeAttr('ClickID');
                        if ($(this).attr('ClickEdit' + Obj.TempID) !== undefined) {
                            $(this).removeAttr('ClickEdit' + Obj.TempID);
                            $(this).click(function () {
                                Obj.showViewInModal(ID);
                            });
                        }
                        else if ($(this).attr('ClickDelete' + Obj.TempID) !== undefined) {
                            $(this).removeAttr('ClickDelete' + Obj.TempID);
                            $(this).click(function () {
                                Obj.DeleteDynamic(ID);
                            });
                        }
                    });

                    $('[RedirectLink' + Obj.TempID + ']').each(function () {
                        var ID = $(this).attr('LinkID');
                        $(this).removeAttr('LinkID');
                        $(this).removeAttr('RedirectLink' + Obj.TempID);
                        $(this).click(function () {
                            Obj.DynamicRowOnClick(ID);
                        });
                    });

                    ChangeLang();

                }
            });

        }
        DynamicSave(AJAX, sucSave);
    }
    else {

        var AJAX = new AJAXsupport();
        AJAX.resetVar();

        AJAX.addData("TempID", this.TempID);
        AJAX.addData("SaveType", 'GetDynamicTree');

        var Obj = this;
        var sucSave = function () {
            $('#' + Obj.GridHolderID).html(AJAX.getRefreshText());
        }

        DynamicSave(AJAX, sucSave);
    }

}


DynamicFormBuilder.prototype.showViewInModal = function (FormID) {
    var AJAX = new AJAXsupport();
    AJAX.resetVar();
    AJAX.addData("TempID", this.TempID);
    if (!FormID) {
        AJAX.addData("FormID", "");

    }
    else {
        AJAX.addData("FormID", FormID);

    }



    AJAX.addData("SaveType", 'GetInspFieldTemplate');

    var Obj = this;
    var sucSave = function () {
        $('#dvDynamic' + Obj.TempID).html(AJAX.getRefreshText());
        $('#DynamicModal' + Obj.TempID).modal('show');
        Obj.JsonForHideShow = JSON.parse(AJAX.getExtraData('JsonHideShow'));
        Obj.JsonHideShowFields(Obj.JsonForHideShow);
        Obj.SetEventOnFields();
        Obj.SetQueryStringOnHidden();


        var BtnSave = $('[ClickSave' + Obj.TempID + ']');
        var ID = BtnSave.attr('ClickID');
        BtnSave.removeAttr('ClickID');
        BtnSave.removeAttr('ClickSave' + Obj.TempID);
        BtnSave.click(function () {
            Obj.SaveDynamicFields(ID);
        });
        ChangeLang();
        Obj.afterFormBuilt(ID);
    }
    DynamicSave(AJAX, sucSave);

}

DynamicFormBuilder.prototype.SaveDynamicFields = function (formID) {
    if (this.IsValidDynamicFields()) {
        var AJAX = new AJAXsupport();
        AJAX.resetVar();
        AJAX.addData("SaveType", 'SaveDynamicFields');
        AJAX.addData("TempID", this.TempID);
        AJAX.addData("formID", formID);

        $('[data-ajax]').each(function () {

            if ($(this).attr('columntype') == 'TextEditor') {
                AJAX.addData(this.id, nicEditors.findEditor(this.id).getContent());
            }
            else {
                AJAX.addData(this.id, this.value);
            }

        });

        var Obj = this;
        var sucSave = function () {
            alert(AJAX.getMessage());
            $('#DynamicModal' + Obj.TempID).modal('hide');
            Obj.showDynamicGrid();
        }
        DynamicSave(AJAX, sucSave);
    }

}

DynamicFormBuilder.prototype.DeleteDynamic = function (formID) {

    if (confirm("Are you sure you want to delete this record?")) {

        var AJAX = new AJAXsupport();
        AJAX.resetVar();
        AJAX.addData("SaveType", 'DeleteDynamicFields');
        AJAX.addData("TempID", this.TempID);
        AJAX.addData("formID", formID);

        var Obj = this;
        var sucSave = function () {
            alert(AJAX.getMessage());
            Obj.showDynamicGrid();
        }
        DynamicSave(AJAX, sucSave);

    }
}





function PopUpWindow(type, ID) {

    var txt;

    if (type == 'DDeptSear') {
        //txt = window.open('../PopupWindows/PgHierarchyWindow.aspx?hf=' + ID + '&value=' + 'txt' + ID, "", 'width=800, height=460,scrollbars=1,resizable=yes');
        txt = window.open('../Compliance/windows/pgBusinessHierarchyWindow.aspx?hf=' + ID + '&value=' + 'txt' + ID, "", 'width=800, height=460,scrollbars=1,resizable=no');
    }
    else if (type == 'Employee') {
        txt = window.open('../LossEvent/PgEmployeeWindow.aspx?value=txt' + ID + '&hf=' + ID + '&ProfileID=', '', 'height=400,width=550,scrollbars=yes, resizable=yes');
    }
    else if (type == 'RNObs') {
        txt = window.open('../Compliance/windows/PgObsWindow.aspx?hf=' + ID + '&value=' + 'txt' + ID + '&InspID=', "", 'width=800, height=460,scrollbars=1,resizable=yes');
    }
    else if (type == 'ControlsPop') {
        txt = window.open('../Compliance/windows/pgObsControlPoint.aspx?hf=' + ID + '&value=' + 'txt' + ID, "", 'width=800, height=460,scrollbars=1,resizable=yes');
    }
    else if (type == 'folder') {
        ID = "";
        //        txt = window.open('../ReportPages/Governence/Window/PgSelectModule.aspx?value=true', "", 'width=800, height=460,scrollbars=1,resizable=yes');
        txt = window.open('../Compliance/libWindows/pgLibRuleSearch.aspx?parentCategory=' + ID + '&parentTitle=' + ID, "", 'width=800, height=460,scrollbars=1,resizable=no');
    }
}


function blockNonNumbers(obj, e, allowDecimal, allowNegative) {
    var key;
    var isCtrl = false;
    var keychar;
    var reg;

    if (window.event) {
        key = e.keyCode;
        isCtrl = window.event.ctrlKey
    }
    else if (e.which) {
        key = e.which;
        isCtrl = e.ctrlKey;
    }

    if (isNaN(key)) return true;

    keychar = String.fromCharCode(key);

    // check for backspace or delete, or if Ctrl was pressed
    if (key == 8 || isCtrl) {
        return true;
    }

    reg = /\d/;
    var isFirstN = allowNegative ? keychar == '-' && obj.value.indexOf('-') == -1 : false;
    var isFirstD = allowDecimal ? keychar == '.' && obj.value.indexOf('.') == -1 : false;

    return isFirstN || isFirstD || reg.test(keychar);
}

DynamicFormBuilder.prototype.showDynamicview = function (FormID) {
    var AJAX = new AJAXsupport();
    AJAX.resetVar();
    AJAX.addData("TempID", this.TempID);
    if (!FormID) {
        AJAX.addData("FormID", "");

    }
    else {
        AJAX.addData("FormID", FormID);

    }



    AJAX.addData("SaveType", 'showDynamicview');

    var Obj = this;
    var sucSave = function () {
        $('#dynamicView').html(AJAX.getRefreshText());
        //$('#DynamicModal' + Obj.TempID).modal('show');
        Obj.JsonForHideShow = JSON.parse(AJAX.getExtraData('JsonHideShow'));
        Obj.JsonHideShowFields(Obj.JsonForHideShow);
        Obj.SetEventOnFields();
        Obj.SetQueryStringOnHidden();


        //  var BtnSave = $('[ClickSave' + Obj.TempID + ']');
        //  var ID = BtnSave.attr('ClickID');
        //  BtnSave.removeAttr('ClickID');
        //  BtnSave.removeAttr('ClickSave' + Obj.TempID);
        //  BtnSave.click(function () {
        //      Obj.SaveDynamicFields(ID);
        //  });
        ChangeLang();
        //Obj.afterFormBuilt(FormID);
    }
    DynamicSave(AJAX, sucSave);

}


//not in modal
DynamicFormBuilder.prototype.showView = function (FormID) {
    var AJAX = new AJAXsupport();
    AJAX.resetVar();
    AJAX.addData("TempID", this.TempID);
    if (!FormID) {
        AJAX.addData("FormID", "");

    }
    else {
        AJAX.addData("FormID", FormID);

    }

    AJAX.addData("SaveType", 'GetInspFieldTemplateView');

    var Obj = this;
    var sucSave = function () {
        $('#dynamicView').html(AJAX.getRefreshText());

        ChangeLang();
        //Obj.afterFormBuilt(FormID);
    }
    DynamicSave(AJAX, sucSave);

}


