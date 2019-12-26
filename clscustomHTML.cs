using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Text;
using System.Collections;

namespace risknucleus.DataLayer.Compliance
{

    public class clscustomHTML : cls_DataLayer
    {
        #region Obselete Methods
        [System.Obsolete("This is a obselete methods", true)]
        public string getRecordsData(string id, string tableID)
        {
            StringBuilder strToret = new StringBuilder();
            DataTable tableInfo = getDataTableFromQuery("select * from SUPP_TablesForTemplate where tableid=" + tableID + "");
            DataTable dtColumns = getDataTableFromQuery("select * from templateconfiguration where tableid=" + tableID + "");

            string qry = "select ";
            foreach (DataRow drcolumns in dtColumns.Rows)
            {
                qry += drcolumns["FieldName"].ToString() + ",";
            }
            qry = qry.Remove(qry.Length - 1);
            qry += " from " + tableInfo.Rows[0]["tableSQLName"].ToString() + " where " + tableInfo.Rows[0]["idcol"].ToString() + "=" + id;

            DataTable dtdata = getDataTableFromQuery(qry);
            string selectValue = "";
            strToret.Append("<table id=\"tblData\" width=\"100%\">");
            int columCount = 0;
            string textboxClass = "largeTextField";
            strToret.Append("<tr>");
            foreach (DataRow dr in dtColumns.Rows)
            {
                if (columCount == 4)
                {
                    strToret.Append("<tr>");
                    columCount = 0;
                }
                strToret.Append("<td class=\"label\">");
                strToret.Append(dr["DisplaycolumnName"].ToString());
                strToret.Append("</td>");

                strToret.Append("<td>");

                // for Date fields
                if (dr["FieldType"].ToString() == "date")
                    textboxClass = "date";
                else
                    textboxClass = "largeTextField";

                if (dtdata.Rows.Count > 0)
                    selectValue = dtdata.Rows[0][dr["FieldName"].ToString()].ToString();



                if (dr["Foreignkey"].ToString() != "Y")
                    strToret.Append("<input type=\"text\" id=\"txt_" + dr["FieldName"].ToString() + "\" class=\"" + textboxClass + "\" value=\"" + selectValue + "\" custom=\"\"/>");
                else
                    // strToret.Append(createDropDown(dr["LinkedTable"].ToString(), dr["CheckFieldName"].ToString() + " +'-'+ " + dr["CheckFieldDesc"].ToString(), dr["ReturnIDFieldName"].ToString(), selectValue, dr["FieldName"].ToString()));

                    strToret.Append("</td>");

                columCount++;

                if (columCount == 4)
                    strToret.Append("</tr>");

            }

            strToret.Append("<tr class=\"buttonSection\"><td colspan=\"8\">");
            strToret.Append("<button id=\"btn_save\" type=\"button\" class=\"normalButton\" onclick=\"SaveRecord()\">Save</button>");
            strToret.Append("<button id=\"btn_del\" type=\"button\" class=\"normalButton\" onclick=\"Action_delete()\">Delete</button>");
            strToret.Append("<button id=\"btn_cancel\" type=\"button\" class=\"normalButton\" onclick=\"Cancel()\">Cancel</button>");
            strToret.Append("</td></tr></table>");

            return strToret.ToString();
        }
        [System.Obsolete("This is a obselete methods", true)]
        public string createDropdown(string table, string displayColumn, string IDColumn, string selected, string DropDownID)
        {
            DataTable dtData = getDataTableFromQuery("select " + displayColumn + "," + IDColumn + " from " + table + "");
            string selectedValue = "";
            StringBuilder strToRet = new StringBuilder();
            strToRet.Append("<select  id=\"ddn_" + DropDownID + "\"  style =\"width:160px;\" class=\"listMenu\" custom=\"\">");
            strToRet.Append("<option value=\"\"></option>");
            foreach (DataRow dr in dtData.Rows)
            {
                if (selected == dr[1].ToString())
                    selectedValue = "selected";
                else selectedValue = "";
                strToRet.Append("<option value=\"" + dr[1].ToString() + "\" " + selectedValue + " >" + dr[0].ToString() + "</option>");

            }
            strToRet.Append("</select>");
            return strToRet.ToString();
        }
        [System.Obsolete("This is a obselete methods", true)]
        public DataTable getTableInfo(string tableID)
        {
            return getDataTableFromQuery("select * from SUPP_TablesForTemplate where tableid=" + tableID + "");
        }
        [System.Obsolete("This is a obselete methods", true)]
        public DataTable getDataforAudit(string tableid, string idcol, string id, string tablename)
        {
            return getDataTableFromQuery("select * from " + tablename + " where " + idcol + " = " + id + "");
        }
        [System.Obsolete("This is a obselete methods", true)]
        public DataTable getTableColumnsInfo(string tableID)
        {
            return getDataTableFromQuery("select * from TemplateConfiguration where tableid=" + tableID + "");
        }
        #endregion

        private string empID = "";
        public clscustomHTML()
        {
            empID = HttpContext.Current.Session["empID"].ToString();
        }

        #region Total Dynamic Fields Functionality

        #region DML Operations

        private void DeleteDynamicFields(string TempID, string FormID, string pageName)
        {
            try
            {
                DataTable dt = GetFieldTemplate(TempID, ForDelete: true);

                if (dt.Rows.Count > 0)
                {
                    string idCol = dt.Rows[0]["idcol"].ToString();

                    DataTable oldValue = getDataTableFromQuery("SELECT * FROM [" + dt.Rows[0]["tableSqlName"].ToString() + "] WHERE [" + idCol + "] = " + checkNull(FormID));
                    oldValue.TableName = dt.Rows[0]["tableSqlName"].ToString();
                    clsAudit p_name = new clsAudit();
                    string userID = HttpContext.Current.Session["empID"].ToString();
                    string auditID = userID.ToString() + DateTime.Now.Ticks.ToString();
                    string pageID = "";
                    string strAction = "D";
                    if (!string.IsNullOrEmpty(pageName))
                    {
                        DataTable dtpageID = getDataTableFromQuery("select pageID pages where pageName = " + checkNull(pageName));
                        if (dtpageID.Rows.Count > 0)
                        {
                            pageID = dtpageID.Rows[0][0].ToString();
                        }
                        
                    }
                    ArrayList audData = new ArrayList();
                    audData.Add(auditID);
                    audData.Add(idCol);
                    audData.Add(pageID);
                    audData.Add(strAction);
                    audData.Add(userID);

                    p_name.addAudit(audData, DateTime.Now);               //fill audit table
                    p_name.addAuditValue(strAction, auditID, oldValue);

                    string[] column = dt.Rows[0]["ExtraLinkedTables"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    string qry = string.Join("", dt.AsEnumerable().Where(dr => !string.IsNullOrEmpty(dr.Field<string>("InsertLinkedTable")) && !string.IsNullOrEmpty(dr.Field<string>("InsertLinkedTableID"))).Select(dr => "DELETE FROM [" + dr.Field<string>("InsertLinkedTable") + "] WHERE [" + dr.Field<string>("InsertLinkedTableID") + "] = " + checkNull(FormID)).ToArray<string>());
                    qry += string.Join("; ", dt.Rows[0]["ExtraLinkedTables"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(dr => "; DELETE FROM [" + dr.Trim() + "] WHERE [" + idCol + "] = " + checkNull(FormID)).ToArray());
                    if (dt.Rows[0]["AuditTable"].ToString() != "")
                        qry += " DELETE FROM [" + dt.Rows[0]["AuditTable"].ToString() + "] WHERE [" + idCol + "] = " + checkNull(FormID);
                    qry += " DELETE FROM [" + dt.Rows[0]["tableSqlName"].ToString() + "] WHERE [" + idCol + "] = " + checkNull(FormID);
                    execute(qry);
                }
            }
            catch (Exception)
            {
                throw new Exception("Record is Linked.");
            }

        }

        private void InsertDynamicFields(string TempID, DataTable dt, string recordID)
        {
            try
            {
                DataTable dtTemp = GetFieldTemplate(TempID);

                List<string> nonMultiFields = dtTemp.Select("IsMuti='False' and IsPrimary='False'").OfType<DataRow>().Select(dr => dr.Field<string>("FieldName")).ToList<string>();

                string primaryColumn = dtTemp.Rows[0]["idcol"].ToString();

                string primaryColumnValue = recordID;

                if (string.IsNullOrEmpty(primaryColumnValue))
                {
                    string qry = "insert into [" + dtTemp.Rows[0]["tableSqlName"].ToString() + "] ([" + string.Join("],[", dt.Rows.OfType<DataRow>().OrderBy(dr => dr.Field<string>("key")).Select(dr => dr.Field<string>("key")).Where<string>(S => nonMultiFields.Contains(S)).ToArray<string>()) + "]) values (" + string.Join(",", dt.AsEnumerable().OrderBy(dr => dr.Field<string>("key")).Where(row => nonMultiFields.Contains(row.Field<string>("key"))).Select(S => checkNull(S.Field<string>("data"))).ToArray<string>()) + "); select SCOPE_IDENTITY();";
                    DataTable Exe = getDataTableFromQuery(qry);

                    if (Exe.Rows.Count > 0)
                    {
                        bool StartWfOnSave = false; bool.TryParse(dtTemp.Rows[0]["StartWfOnSave"].ToString(), out StartWfOnSave);
                        bool MailOnStartWf = false; bool.TryParse(dtTemp.Rows[0]["MailOnStartWf"].ToString(), out MailOnStartWf);

                        primaryColumnValue = Exe.Rows[0][0].ToString();

                        risknucleus.BuisnessLayer.WorkFlow.clsGenWorkFlow gen = new BuisnessLayer.WorkFlow.clsGenWorkFlow();
                        if (StartWfOnSave)
                        {
                            gen.startWf(primaryColumnValue, int.Parse(dtTemp.Rows[0]["WfType"].ToString()), "");
                        }
                        if (MailOnStartWf)
                        {
                            gen.sendEmail(primaryColumnValue, dtTemp.Rows[0]["FirstWfStep"].ToString(), "", "", empID, dtTemp.Rows[0]["WfType"].ToString(), "", "", "");
                        }

                        if (!string.IsNullOrEmpty(dtTemp.Rows[0]["AuditTable"].ToString()))
                        {
                            execute("INSERT INTO [" + dtTemp.Rows[0]["AuditTable"].ToString() + "] ([" + primaryColumn + "], [change], [changeBy], [changeDate]) VALUES (" + checkNull(primaryColumnValue) + ", 'New " + dtTemp.Rows[0]["tableName"].ToString() + " has been added.' ,'" + empID + "', getdate())");
                        }
                    }

                }
                else
                {
                    DataTable dtActualValue = GetFormInfo(dtTemp, primaryColumnValue);

                    string qry = "Update [" + dtTemp.Rows[0]["tableSqlName"].ToString() + "] set " + string.Join(",", dt.AsEnumerable().Where(dr => nonMultiFields.Contains(dr.Field<string>("key"))).Select(dr => " [" + dr.Field<string>("key") + "] = " + checkNull(dr.Field<string>("data"))).ToArray<string>()) + " where [" + primaryColumn + "] = " + checkNull(primaryColumnValue);

                    execute(qry);

                    if (!string.IsNullOrEmpty(dtTemp.Rows[0]["AuditTable"].ToString()))
                    {
                        updateDynamicAuditTrail(primaryColumn, primaryColumnValue, dtActualValue, dt, dtTemp);
                    }
                }

                if (!string.IsNullOrEmpty(primaryColumnValue))
                {
                    updateDynamicLinkage(dtTemp, dt, primaryColumnValue);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void updateDynamicLinkage(DataTable dtTemp, DataTable dt, string ID)
        {

            try
            {
                StringBuilder qry = new StringBuilder();

                foreach (DataRow dr in dtTemp.Select("IsMuti='True'"))
                {
                    if (!string.IsNullOrEmpty(dr["InsertLinkedTable"].ToString()) && !string.IsNullOrEmpty(dr["InsertLinkedTableID"].ToString()))
                    {
                        string[] ExtraColumnData = dr["ExtralinkedColumn"].ToString().Split(new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
                        qry.Append(" delete from [" + dr["InsertLinkedTable"].ToString() + "] where [" + dr["InsertLinkedTableID"].ToString() + "] = '" + ID + "' " + string.Join("", ExtraColumnData.Where(i => i.IndexOf('º') > -1).Select(i => " and [" + i.Split(',')[0].Replace("º", "") + "] = " + i.Split(',')[1].Replace("{EMPID}", empID)).ToArray()) + "; ");

                        DataTable DtSelectedData = dt.Select("key = '" + dr["FieldName"].ToString() + "'").CopyToDataTable();
                        if (DtSelectedData.Rows.Count > 0)
                        {
                            string[] Val = DtSelectedData.Rows[0]["data"].ToString().Split(new char[] { 'º' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string item in Val)
                            {
                                qry.Append(" insert into [" + dr["InsertLinkedTable"].ToString() + "]([" + dr["InsertLinkedTableID"].ToString() + "],[" + dr["InsertLinkedTableValue"].ToString() + "] " + string.Join("", ExtraColumnData.OrderBy(i => i).Select(i => ", [" + i.Split(',')[0].Replace("º", "") + "]").ToArray()) + ") values (" + checkNull(ID) + ", " + checkNull(item) + " " + string.Join("", ExtraColumnData.OrderBy(i => i).Select(i => ", " + i.Split(',')[1].Replace("{EMPID}", empID)).ToArray()) + ")");
                            }
                        }
                    }

                }

                if (qry.ToString() != "")
                    execute(qry.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void updateDynamicAuditTrail(string IDCol, string IDVal, DataTable dtActualValue, DataTable updatedValue, DataTable TempateTable)
        {
            StringBuilder qry = new StringBuilder();
            foreach (DataColumn DC in dtActualValue.Columns)
            {
                var dr_ = updatedValue.AsEnumerable().Where(dr => dr.Field<string>("Key").Equals(DC.ColumnName));
                if (dr_.Count() > 0)
                {
                    string OldValue = dtActualValue.Rows[0][DC.ColumnName].ToString();
                    string NewValue = dr_.Select(_dr => _dr.Field<string>("data")).ToList()[0];
                    if (!OldValue.Equals(NewValue))
                    {
                        string FieldName = TempateTable.AsEnumerable().Where(_dr => _dr.Field<string>("FieldName").Equals(DC.ColumnName)).Select(_dr => _dr.Field<string>("DisplayColumnName")).ToList()[0];
                        qry.Append("INSERT INTO [" + TempateTable.Rows[0]["AuditTable"].ToString() + "] ([" + IDCol + "], [change], [changeBy], [changeDate]) VALUES (" + checkNull(IDVal) + ", '" + FieldName + " changed from " + (string.IsNullOrEmpty(OldValue) ? "empty" : chkText(OldValue)) + " to " + (string.IsNullOrEmpty(NewValue) ? "empty" : chkText(NewValue)) + ".' ," + checkNull(empID) + ", getdate()); ");
                    }
                }
            }

            if (!string.IsNullOrEmpty(qry.ToString()))
            {
                execute(qry.ToString());
            }

        }

        #endregion

        #region Data Operations

        public DataTable GetFieldTemplate(string tempid, bool ForGrid = false, bool ForDelete = false, bool ForTree = false, bool ForAuditTrial = false)
        {
            return getDataTableFromQuery(string.Format(@"select {0}
                                from TemplateConfiguration Col 
                                inner join [SUPP_TablesForTemplate] TB on Col.TableID = TB.[TableID] 
                                where Col.TableID= '" + tempid + "' and Col.Display = '1' "
                                                      + (ForGrid || ForTree ? " and Col.ForGrid = '1' " : "")
                                                      + " order by Col.Seq", (ForDelete ? //IF DELETE
                                                                                        @"TB.tableSqlName,
                                                                                          TB.AuditTable, 
                                                                                          TB.idcol,
                                                                                          TB.ExtraLinkedTables,
                                                                                          Col.[InsertLinkedTable],
                                                                                          Col.[InsertLinkedTableID]" :
                                                                                            (ForTree ? //ELSE IF TREE
                                                                                                  @"TB.tableSqlName, 
                                                                                                    TB.idcol,
                                                                                                    TB.ParentIDCol,
                                                                                                    Col.FieldName" :
                                                                                                        (ForAuditTrial ? //ELSE IF AUDIT TRAIL
                                                                                                            @"TB.AuditTable,
                                                                                                              TB.idcol" : //ELSE DEFAULT
                                                                                                                    @"Col.ID , 
                                                                                                                    TB.tableSqlName, 
                                                                                                                    TB.idcol,
                                                                                                                    TB.tableName,
                                                                                                                    TB.TotalColSpan,
                                                                                                                    TB.IsCheckForGrid,
                                                                                                                    TB.FirstWfStep,
                                                                                                                    TB.MailOnStartWf,
                                                                                                                    TB.EmailCode,
                                                                                                                    TB.StartWfOnSave,
                                                                                                                    TB.WfType,
                                                                                                                    TB.ParentIDCol,
                                                                                                                    TB.AuditTable,
                                                                                                                    TB.ExtraWhereClause,
                                                                                                                    Col.Seq,
                                                                                                                    Col.FieldName,
                                                                                                                    Col.DisplayColumnName,
                                                                                                                    Col.FieldType,
                                                                                                                    Col.Query, 
                                                                                                                    Col.Mandetory, 
                                                                                                                    Col.FieldLength, 
                                                                                                                    Col.IsDisable,
                                                                                                                    Col.PopUpKey, 
                                                                                                                    Col.IsMuti, 
                                                                                                                    Col.IsPrimary, 
                                                                                                                    Col.[InsertLinkedTable], 
                                                                                                                    Col.[InsertLinkedTableID], 
                                                                                                                    Col.[InsertLinkedTableValue],
                                                                                                                    Col.ColSpan,
                                                                                                                    Col.[LinkedTable], 
                                                                                                                    Col.[CheckFieldName], 
                                                                                                                    Col.[ReturnIDFieldName],
                                                                                                                    Col.[DefaultValue],
                                                                                                                    Col.DefaultValueFromQString,
                                                                                                                    Col.FilterColumn,
                                                                                                                    Col.ExtralinkedColumn,
                                                                                                                    Col.reqTrans")))));
        }

        private DataTable GetFormInfo(DataTable dtTemp, string FormID = "")
        {

            string qry = "Select " + string.Join(",", dtTemp.Select("IsMuti='False'").OfType<DataRow>().Select(dr =>
                                                    (dr.Field<string>("FieldType") == "D" ?
                                                        "convert(varchar, [" + dr.Field<string>("FieldName") + "], 101) " + "[" + dr.Field<string>("FieldName") + "]" :
                                                        (dr.Field<string>("FieldType") == "PopUp" ?
                                                            string.Format(@"(SELECT B.[{3}] FROM [{2}] B WHERE B.[{4}] = A.[{0}]) [TXT{1}], A.[{0}]", dr.Field<string>("FieldName"), dr.Field<string>("FieldName"), dr.Field<string>("LinkedTable"), dr.Field<string>("ReturnIDFieldName"), dr.Field<string>("CheckFieldName")) :
                                                                "A.[" + dr.Field<string>("FieldName") + "]"))).ToArray<string>()) + ",  ";

            foreach (DataRow dr in dtTemp.Select("IsMuti='True'"))
            {
                string InsertLinkedTable = dr["InsertLinkedTable"].ToString();
                string InsertLinkedTableColumn = dr["InsertLinkedTableValue"].ToString();
                string InsertLinkedTableIDColumn = dr["InsertLinkedTableID"].ToString();
                string LinkedTable = dr["LinkedTable"].ToString();
                string DisplaylinkedTableColumn = dr["ReturnIDFieldName"].ToString();
                string CheckFieldNameColumn = dr["CheckFieldName"].ToString();
                string FieldName = dr["FieldName"].ToString();
                string[] ExtraColumnData = dr["ExtralinkedColumn"].ToString().Split(new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries);

                qry += string.Format(@" STUFF(REPLACE((SELECT '#!' + LTRIM(RTRIM(C.[{6}])) AS 'data()' FROM [{0}] B INNER JOIN [{5}] C on B.[{2}] = C.[{7}] WHERE B.[{2}] = A.[{3}] {8}
                        FOR XML PATH('')),' #!',', '), 1, 2, '') as [TXT{4}], ", InsertLinkedTable, InsertLinkedTableColumn, InsertLinkedTableIDColumn, dtTemp.Rows[0]["idcol"].ToString(), FieldName, LinkedTable, DisplaylinkedTableColumn, CheckFieldNameColumn, string.Join("", ExtraColumnData.Where(i => i.IndexOf('º') > -1).Select(i => " and B.[" + i.Split(',')[0].Replace("º", "") + "] = " + i.Split(',')[1].Replace("{EMPID}", empID)).ToArray()));

                qry += string.Format(@" STUFF(REPLACE((SELECT '#!' + LTRIM(RTRIM(B.[{1}])) AS 'data()' FROM [{0}] B WHERE B.[{2}] = A.[{3}] {5}
                        FOR XML PATH('')),' #!',', '), 1, 2, '') as [{4}], ", InsertLinkedTable, InsertLinkedTableColumn, InsertLinkedTableIDColumn, dtTemp.Rows[0]["idcol"].ToString(), FieldName, string.Join("", ExtraColumnData.Where(i => i.IndexOf('º') > -1).Select(i => " and B.[" + i.Split(',')[0].Replace("º", "") + "] = " + i.Split(',')[1].Replace("{EMPID}", empID)).ToArray()));
            }

            qry += " '' [XXX] from  [" + dtTemp.Rows[0]["tableSqlName"].ToString() + "] A " + (string.IsNullOrEmpty(FormID) ? "" : "where A.[" + dtTemp.Rows[0]["idcol"].ToString() + "] = '" + FormID + "'") + "; ";

            return getDataTableFromQuery(qry);
        }

        private DataTable getBasicProp(string TempID)
        {
            return getDataTableFromQuery("select ForTree,isAdd,isEdit,isDelete,AddbtnText,TableName,ModalWidth from SUpp_TablesForTemplate where tableID = " + checkNull(TempID));
        }

        private string GetJsonForHideShowFields(string TempID)
        {
            string qry = @"select Field.FieldName, Link.Value, FieldToHide.FieldName [FieldToHide] from Link_Column_Hide link
                           inner join TemplateConfiguration Field on Field.ID = link.ColumnID
                           inner join TemplateConfiguration FieldToHide on FieldToHide.ID = link.ColumnToHideID";

            DataTable dt = getDataTableFromQuery(qry);

            return GetJson(dt);
        }

        #endregion

        #region View Operations

        private string GetFieldTemplate(string tempid, string FormID, string pageLink)
        {

            StringBuilder boxhtml = new StringBuilder();
            string queryValues = "", queryTxt = "", Valid = "", Disable = "", PopUpImg = "";
            int columCount = 0;
            DataTable dt = GetFieldTemplate(tempid);
            DataTable dtInfo = new DataTable();
            int totalColSpan = Convert.ToInt32(dt.Rows[0]["TotalColSpan"]);
            double tdPercent = 100 / totalColSpan;
            if (!string.IsNullOrEmpty(FormID))
            {
                dtInfo = GetFormInfo(dt, FormID);
            }

            if (dt.Rows.Count > 0)
            {
                boxhtml.Append("<div class=\"modal-body\" style=\"padding: initial;\">");
                boxhtml.Append("<table style=\"width: 100%\" class=\"cellgap\">");

                foreach (DataRow dr in dt.Rows)
                {
                    int colspan = 1; int.TryParse(dr["colspan"].ToString(), out colspan);
                    int randomID = new Random().Next();

                    if (dtInfo.Rows.Count > 0)
                    {
                        queryValues = dtInfo.Rows[0][dr["FieldName"].ToString()].ToString();

                        if (dr["FieldType"].ToString() == "PopUp")
                        {
                            queryTxt = dtInfo.Rows[0]["TXT" + dr["FieldName"].ToString()].ToString();
                        }
                        else
                        {
                            queryTxt = "";
                        }
                    }

                    if (dr["Mandetory"].ToString() == "Y")
                        Valid = "valid-ajax='vld" + dr["FieldName"].ToString() + randomID + "'";
                    else
                        Valid = "";

                    if (dr["IsDisable"].ToString() == "True")
                        Disable = "disabled";
                    else
                        Disable = "";


                    if (columCount == totalColSpan && dr["FieldType"].ToString() != "Hidden")
                    {
                        boxhtml.Append("<tr>");
                        columCount = 0;
                    }
                    else if (dr["FieldType"].ToString() == "TextEditor")
                    {
                        boxhtml.Append("</tr><tr>");
                        columCount = totalColSpan;
                    }
                    //else
                    //{
                    columCount = columCount + colspan;
                    //}

                    // set popup image
                    if (dr["FieldType"].ToString() == "PopUp")
                        PopUpImg = "<img style='cursor: hand' onclick=\"PopUpWindow('" + dr["PopUpKey"].ToString() + "', '" + dr["FieldName"].ToString() + "');\" alt='Search' src='../../images/search.png'>";
                    else
                        PopUpImg = "";


                    if (dr["FieldType"].ToString() != "Hidden")
                        // Label 
                        boxhtml.Append("<td style='width: " + (tdPercent * colspan) + "%' colspan=\"" + colspan + "\" id='td" + dr["FieldName"].ToString() + "' > <div class=\"form-group\">   <label class=\"control-label\" lang=\"" + dr["DisplayColumnName"].ToString().Replace(" ", "_") + "\">" + dr["DisplayColumnName"].ToString() + "</label> " + PopUpImg + " <span id='vld" + dr["FieldName"].ToString() + randomID + "' style='display:none; color:red'>*</span>");



                    if (dr["FieldType"].ToString() == "D") // Date Field
                    {
                        boxhtml.Append("<input type=\"text\" readonly data-ajax columnType='" + dr["FieldType"].ToString() + "' " + Valid + " " + Disable + "  id=\"" + dr["FieldName"].ToString() + "\"  class=\"form-control form-control-inline date-picker\" value=\"" + queryValues + "\" /></div> </td>");
                    }
                    else if (dr["FieldType"].ToString() == "A") // Amount Field
                    {
                        boxhtml.Append("<input type=\"text\" data-ajax columnType='" + dr["FieldType"].ToString() + "' " + Valid + " " + Disable + " id=\"" + dr["FieldName"].ToString() + "\"  class=\"form-control\" onkeypress=\"return blockNonNumbers(this, event, false, true);\" onfocus=\"removeCommasR(this.value,this.id);\" onblur=\"CurrencyFormattedR(this.value,this.id)\" onkeyup=\"zeroPaddingR(event.keyCode,this.id)\" maxlength='" + dr["FieldLength"].ToString() + "' value=\"" + queryValues + "\"/></div> </td>");
                    }
                    else if (dr["FieldType"].ToString() == "I") // Int Field
                    {
                        boxhtml.Append("<input type=\"text\" data-ajax columnType='" + dr["FieldType"].ToString() + "' " + Valid + " " + Disable + " id=\"" + dr["FieldName"].ToString() + "\"  class=\"form-control\" onkeypress=\"return blockNonNumbers(this, event, false, true);\" maxlength='" + dr["FieldLength"].ToString() + "' value=\"" + queryValues + "\"  /></div> </td>");
                    }
                    else if (dr["FieldType"].ToString() == "DD")// Dropdown Field
                    {
                        if (dr["Query"].ToString().Trim() != "")
                        {
                            // boxhtml.Append(createDropDown_selected(getDataTableFromQuery(dr["Query"].ToString()), dr["FieldName"].ToString(), Valid + " " + Disable + " " + "columnType='" + dr["FieldType"].ToString() + "' data-ajax", true, queryValues, true) + "</td>");

                            DataTable dtdata = getDataTableFromQuery(dr["Query"].ToString());
                            if (getDataTableFromQuery("select Langselected from userlanguage where empid='" + HttpContext.Current.Session["empid"].ToString() + "'").Rows[0][0].ToString() != "E")
                            {
                                if (dr["reqTrans"].ToString() == "True")
                                {
                                    // dtdata = getDataTableFromQuery("select [" + dr["CheckFieldName"].ToString() + "], ISNULL((select top 1 turkish from Translation where term= " + dr["ReturnIDFieldName"].ToString() + ")," + dr["ReturnIDFieldName"].ToString() + ")   from " + dr["LinkedTable"].ToString() + " " + (!string.IsNullOrEmpty(FilterColumn) && !string.IsNullOrEmpty(FilterColumnValue) ? " WHERE [" + FilterColumn + "] = " + checkNull(FilterColumnValue) : ""));
                                    boxhtml.Append(createDropDown_selected(dtdata, dr["FieldName"].ToString(), Valid + " " + Disable + " " + "columnType='" + dr["FieldType"].ToString() + "' data-ajax", true, queryValues, true) + "</td>");
                                }

                                else
                                    boxhtml.Append(createDropDownW(dtdata, dr["FieldName"].ToString(), Valid + " " + Disable + " " + "columnType='" + dr["FieldType"].ToString() + "' data-ajax", true, queryValues, true) + "</td>");
                            }
                            else
                                boxhtml.Append(createDropDownW(dtdata, dr["FieldName"].ToString(), Valid + " " + Disable + " " + "columnType='" + dr["FieldType"].ToString() + "' data-ajax", true, queryValues, true) + "</td>");


                        }
                        else
                        {
                            string FilterColumn = dr["FilterColumn"].ToString();
                            string FilterColumnValue = "";

                            if (!string.IsNullOrEmpty(dr["DefaultValueFromQString"].ToString()))
                            {
                                Uri uri = new Uri(pageLink);
                                FilterColumnValue = HttpUtility.ParseQueryString(uri.Query).Get(dr["DefaultValueFromQString"].ToString());
                            }

                            // For Translation ---------------
                            DataTable dtdata = getDataTableFromQuery("select [" + dr["CheckFieldName"].ToString() + "], [" + dr["ReturnIDFieldName"].ToString() + "]  from " + dr["LinkedTable"].ToString() + " " + (!string.IsNullOrEmpty(FilterColumn) && !string.IsNullOrEmpty(FilterColumnValue) ? " WHERE [" + FilterColumn + "] = " + checkNull(FilterColumnValue) : ""));
                            if (getDataTableFromQuery("select Langselected from userlanguage where empid='" + HttpContext.Current.Session["empid"].ToString() + "'").Rows[0][0].ToString() != "E")
                            {
                                if (dr["reqTrans"].ToString() == "True")
                                {
                                    dtdata = getDataTableFromQuery("select [" + dr["CheckFieldName"].ToString() + "], ISNULL((select top 1 turkish from Translation where term= " + dr["ReturnIDFieldName"].ToString() + ")," + dr["ReturnIDFieldName"].ToString() + ")   from " + dr["LinkedTable"].ToString() + " " + (!string.IsNullOrEmpty(FilterColumn) && !string.IsNullOrEmpty(FilterColumnValue) ? " WHERE [" + FilterColumn + "] = " + checkNull(FilterColumnValue) : ""));
                                    boxhtml.Append(createDropDown_selected(dtdata, dr["FieldName"].ToString(), Valid + " " + Disable + " " + "columnType='" + dr["FieldType"].ToString() + "' data-ajax", true, queryValues, true) + "</td>");
                                }

                                else
                                    boxhtml.Append(createDropDownW(dtdata, dr["FieldName"].ToString(), Valid + " " + Disable + " " + "columnType='" + dr["FieldType"].ToString() + "' data-ajax", true, queryValues, true) + "</td>");
                            }
                            else
                                boxhtml.Append(createDropDownW(dtdata, dr["FieldName"].ToString(), Valid + " " + Disable + " " + "columnType='" + dr["FieldType"].ToString() + "' data-ajax", true, queryValues, true) + "</td>");
                        }
                    }
                    else if (dr["FieldType"].ToString() == "TXT") // TextBox Field
                    {
                        boxhtml.Append("<input type=\"text\" data-ajax columnType='" + dr["FieldType"].ToString() + "' " + Valid + " " + Disable + " id=\"" + dr["FieldName"].ToString() + "\"  class=\"form-control\"  maxlength='" + dr["FieldLength"].ToString() + "' value=\"" + queryValues + "\"/></div> </td>");
                    }
                    else if (dr["FieldType"].ToString() == "TextArea") // TextArea Field
                    {
                        boxhtml.Append("<textarea data-ajax columnType='" + dr["FieldType"].ToString() + "' " + Valid + " " + Disable + " id=\"" + dr["FieldName"].ToString() + "\"  class=\"form-control\" >" + queryValues + "</textarea></td>");
                    }
                    else if (dr["FieldType"].ToString() == "TextEditor") // Editor Field
                    {
                        boxhtml.Append("<div><textarea data-ajax columnType='" + dr["FieldType"].ToString() + "' " + Valid + " " + Disable + " id=\"" + dr["FieldName"].ToString() + "\"  class=\"form-control\" >" + queryValues + "</textarea></div></td>");
                        boxhtml.Append(@"<script type='text/javascript'>
                                                var editor1 = new nicEditor({
                                                                  iconsPath: BasePath + 'library/Images/nicEditorIcons.gif',
                                                                  maxHeight: 300
                                                              }).panelInstance('" + dr["FieldName"].ToString() + "'); $('#" + randomID + " [unselectable]').first().css('width', '100%');$('.nicEdit-main').css('text-align', 'Left');$('.nicEdit-main').css('height', '150px');$('.nicEdit-main').css('width', '100%');$('.nicEdit-main').parent().css('width', '100%');$('.nicEdit-main ').css('height', '150px');</script>");
                    }
                    else if (dr["FieldType"].ToString() == "PopUp") // PopUp Field
                    {
                        boxhtml.Append("<input type=\"text\" readonly id=\"txt" + dr["FieldName"].ToString() + "\"  class=\"form-control\"  value=\"" + queryTxt + "\"/></div> ");
                        boxhtml.Append("<input type=\"hidden\" data-ajax columnType='" + dr["FieldType"].ToString() + "' " + Valid + " id=\"" + dr["FieldName"].ToString() + "\"  class=\"form-control\"  value=\"" + queryValues + "\"/></div> </td>");
                    }
                    else if (dr["FieldType"].ToString() == "Hidden")
                    {
                        boxhtml.Append("<input type=\"hidden\" data-ajax columnType='" + dr["FieldType"].ToString() + "' id=\"" + dr["FieldName"].ToString() + "\" " + (string.IsNullOrEmpty(dr["DefaultValueFromQString"].ToString()) ? "" : "QString") + "  value=\"" + (string.IsNullOrEmpty(dr["DefaultValueFromQString"].ToString()) ? dr["DefaultValue"].ToString() : dr["DefaultValueFromQString"].ToString()) + "\"/>");
                    }

                    if (columCount == totalColSpan && dr["FieldType"].ToString() != "Hidden")
                        boxhtml.Append("</tr>");

                }
                boxhtml.Append("</table>");
                boxhtml.Append("</div>");
                boxhtml.Append("<div class=\"modal-footer\">"
                   + " <input class=\"btn green\" type=\"button\" lang=\"Save\" id=\"btnSave\" ClickSave" + tempid + " ClickID=\"" + FormID + "\" value=\"" + getTranslatedValue("Save") + "\" /> "
                   + " <input class=\"btn btn-outline dark\" data-dismiss=\"modal\" lang=\"Close\" type=\"button\" id=\"Button2\" value=\"Close\" />"
                   + " </div>");

                boxhtml.Append(@"<script type='text/javascript'>
                                    jq162('.date-picker').datepicker({
                                        changeMonth: true,
                                        changeYear: true,
                                        dateFormat: 'dd M yy'
                                    });
                                </script>");

            }

            return boxhtml.ToString();
        }

        private string getHeaderfortable(string tempid)
        {
            DataTable dtdata = getDataTableFromQuery("select DisplayColumnName from [TemplateConfiguration] where TableID = '" + tempid + "' and ForGrid = '1' order by Seq");

            StringBuilder strtoret = new StringBuilder();

            strtoret.Append("<table class=\"table table-checkable tbldata\">");
            strtoret.Append(" <thead><tr>");
            //strtoret.Append("<th></th>");
            strtoret.Append("<th></th>");
            foreach (DataRow dr in dtdata.Rows)
            {
                strtoret.Append("<th>");
                strtoret.Append("<span lang='" + dr["DisplayColumnName"].ToString().Replace(" ", "_") + "'>" + getTranslatedValue(dr["DisplayColumnName"].ToString()) + "</span>");
                strtoret.Append("</th>");

            }

            strtoret.Append("</tr></thead> <tbody><tr><td colspan=" + dtdata.Rows.Count + " class=\"dataTables_empty\">Loading data from server</td></tr></tbody>");
            strtoret.Append("</table>");

            return strtoret.ToString();
        }

        private string getDynamicTree(string TempID)
        {

            DataTable dt = GetFieldTemplate(TempID, ForTree: true);

            if (dt.Rows.Count > 0)
            {
                string TreeID = dt.Rows[0]["tableSqlName"].ToString();
                string qry = "SELECT [" + dt.Rows[0]["idcol"].ToString() + "] as ID, " + (string.IsNullOrEmpty(dt.Rows[0]["ParentIDCol"].ToString()) ? "null" : "[" + dt.Rows[0]["ParentIDCol"].ToString() + "]") + " as parentID, " + string.Join(" + ' - ' + ", dt.AsEnumerable().Select(dr => " cast([" + dr.Field<string>("FieldName") + "] as varchar) ").ToArray<string>()) + " as value, 'A' as status from [" + dt.Rows[0]["tableSqlName"].ToString() + "]";
                string TableName = dt.Rows[0]["tableSqlName"].ToString();
                string Title = dt.Rows[0]["tableSqlName"].ToString();
                string ParentField = "parentID";
                bool isHierarchy = true;
                bool add = true;
                bool edit = true;
                bool del = true;
                bool select = true;

                return getTreeView(TreeID, qry, TableName, Title, ParentField, isHierarchy, add, edit, del, select);
            }

            return "";
        }

        public string getDynamicAuditTrail(string TempID, string FormID)
        {
            DataTable dtTemp = GetFieldTemplate(TempID, ForAuditTrial: true);

            if (dtTemp.Rows.Count > 0)
            {
                string retStr = "";
                string TableName = dtTemp.Rows[0]["AuditTable"].ToString();
                string idCol = dtTemp.Rows[0]["idcol"].ToString();

                if (TableName != "")
                {

                    DataTable dt = getDataTableFromQuery(string.Format(@"select change,firstName+' '+lastName+' ('+ChangeBy+')' as EMP,changeDate from {0} 
                                                   left join users on empid=changeBy where {1} = " + checkNull(FormID) + " order by changeDate", TableName, idCol));

                    retStr = "<ul class=\"chats\"><li></li> ";

                    foreach (DataRow dr in dt.Rows)
                    {
                        retStr += "<li class=\"in\"><img src=\"../../../Images/user_comment.png\" class=\"avatar\"><div class=\"message\"> <span class=\"arrow\" readonly=\"false\"></span><span class=\"datetime\" readonly=\"false\"></span>";
                        retStr += "<a class=\"user_profile\">" + dr["EMP"].ToString() + "</a><br /><br /><span class=\"body\" readonly=\"false\">" + dr["Change"].ToString() + "</span><br /><span class=\"datesec\" readonly=\"false\">" + Convert.ToDateTime(dr["changeDate"]).ToLongDateString() + "</span></div> </li>";

                    }
                    retStr += "</ul>";
                }
                return retStr;
            }

            return "";
        }

        private string getDynamicVIEWONLY(string tempid, string FormID, string pageLink)
        {

            StringBuilder boxhtml = new StringBuilder();
            string queryValues = "", queryTxt = "", Valid = "", Disable = "", PopUpImg = "";
            int columCount = 0;
            DataTable dt = GetFieldTemplate(tempid);
            DataTable dtInfo = new DataTable();
            int totalColSpan = Convert.ToInt32(dt.Rows[0]["TotalColSpan"]);
            double tdPercent = 100 / totalColSpan;
            if (!string.IsNullOrEmpty(FormID))
            {
                dtInfo = GetFormInfo(dt, FormID);
            }

            if (dt.Rows.Count > 0)
            {
                // boxhtml.Append("<div class=\"modal-body\" style=\"padding: initial;\">");
                boxhtml.Append("<table style=\"width: 100%\" class=\"cellgap\">");

                foreach (DataRow dr in dt.Rows)
                {
                    int colspan = 1; int.TryParse(dr["colspan"].ToString(), out colspan);
                    int randomID = new Random().Next();

                    if (dtInfo.Rows.Count > 0)
                    {
                        queryValues = dtInfo.Rows[0][dr["FieldName"].ToString()].ToString();

                        if (dr["FieldType"].ToString() == "PopUp")
                        {
                            queryTxt = dtInfo.Rows[0]["TXT" + dr["FieldName"].ToString()].ToString();
                        }
                        else
                        {
                            queryTxt = "";
                        }
                    }

                    if (dr["Mandetory"].ToString() == "Y")
                        Valid = "valid-ajax='vld" + dr["FieldName"].ToString() + randomID + "'";
                    else
                        Valid = "";

                    if (dr["IsDisable"].ToString() == "True")
                        Disable = "disabled";
                    else
                        Disable = "";


                    if (columCount == totalColSpan && dr["FieldType"].ToString() != "Hidden")
                    {
                        boxhtml.Append("<tr>");
                        columCount = 0;
                    }
                    else if (dr["FieldType"].ToString() == "TextEditor")
                    {
                        boxhtml.Append("</tr><tr>");
                        columCount = totalColSpan;
                    }
                    //else
                    //{
                    columCount = columCount + colspan;
                    //}

                    // set popup image
                    // if (dr["FieldType"].ToString() == "PopUp")
                    // PopUpImg = "<img style='cursor: hand' onclick=\"PopUpWindow('" + dr["PopUpKey"].ToString() + "', '" + dr["FieldName"].ToString() + "');\" alt='Search' src='../../images/search.png'>";
                    // else
                    //   PopUpImg = "";


                    if (dr["FieldType"].ToString() != "Hidden")
                        // Label 
                        boxhtml.Append("<td style='width: " + (tdPercent * colspan) + "%' colspan=\"" + colspan + "\" id='td" + dr["FieldName"].ToString() + "' > <div class=\"form-group\">   <label class=\"control-label\" lang=\"" + dr["DisplayColumnName"].ToString().Replace(" ", "_") + "\">" + dr["DisplayColumnName"].ToString() + "</label> " + PopUpImg + " <span id='vld" + dr["FieldName"].ToString() + randomID + "' style='display:none; color:red'>*</span>");



                    if (dr["FieldType"].ToString() == "D") // Date Field
                    {
                        boxhtml.Append("<span type=\"text\" readonly data-ajax columnType='" + dr["FieldType"].ToString() + "' " + Valid + " " + Disable + "  id=\"" + dr["FieldName"].ToString() + "\"  value=\"" + queryValues + "\" ></span></div> </td>");
                    }
                    else if (dr["FieldType"].ToString() == "A") // Amount Field
                    {
                        boxhtml.Append("<span type=\"text\" data-ajax columnType='" + dr["FieldType"].ToString() + "' " + Valid + " " + Disable + " id=\"" + dr["FieldName"].ToString() + "\"  onkeypress=\"return blockNonNumbers(this, event, false, true);\" onfocus=\"removeCommasR(this.value,this.id);\" onblur=\"CurrencyFormattedR(this.value,this.id)\" onkeyup=\"zeroPaddingR(event.keyCode,this.id)\" maxlength='" + dr["FieldLength"].ToString() + "' value=\"" + queryValues + "\"></span></div> </td>");
                    }
                    else if (dr["FieldType"].ToString() == "I") // Int Field
                    {
                        boxhtml.Append("<span type=\"text\" data-ajax columnType='" + dr["FieldType"].ToString() + "' " + Valid + " " + Disable + " id=\"" + dr["FieldName"].ToString() + "\"  onkeypress=\"return blockNonNumbers(this, event, false, true);\" maxlength='" + dr["FieldLength"].ToString() + "' value=\"" + queryValues + "\"  ></span></div> </td>");
                    }
                    else if (dr["FieldType"].ToString() == "DD")// Dropdown Field
                    {
                        if (dr["Query"].ToString().Trim() != "")
                        {
                            // boxhtml.Append(createDropDown_selected(getDataTableFromQuery(dr["Query"].ToString()), dr["FieldName"].ToString(), Valid + " " + Disable + " " + "columnType='" + dr["FieldType"].ToString() + "' data-ajax", true, queryValues, true) + "</td>");

                            DataTable dtdata = getDataTableFromQuery(dr["Query"].ToString());
                            if (getDataTableFromQuery("select Langselected from userlanguage where empid='" + HttpContext.Current.Session["empid"].ToString() + "'").Rows[0][0].ToString() != "E")
                            {
                                if (dr["reqTrans"].ToString() == "True")
                                {
                                    // dtdata = getDataTableFromQuery("select [" + dr["CheckFieldName"].ToString() + "], ISNULL((select top 1 turkish from Translation where term= " + dr["ReturnIDFieldName"].ToString() + ")," + dr["ReturnIDFieldName"].ToString() + ")   from " + dr["LinkedTable"].ToString() + " " + (!string.IsNullOrEmpty(FilterColumn) && !string.IsNullOrEmpty(FilterColumnValue) ? " WHERE [" + FilterColumn + "] = " + checkNull(FilterColumnValue) : ""));
                                    boxhtml.Append(createSpanfromselectDD(dtdata, dr["FieldName"].ToString(), Valid + " " + Disable + " " + "columnType='" + dr["FieldType"].ToString() + "' data-ajax", true, queryValues, true) + "</td>");
                                }

                                else
                                    boxhtml.Append(createSpanfromselectDD(dtdata, dr["FieldName"].ToString(), Valid + " " + Disable + " " + "columnType='" + dr["FieldType"].ToString() + "' data-ajax", true, queryValues, true) + "</td>");
                            }
                            else
                                boxhtml.Append(createSpanfromselectDD(dtdata, dr["FieldName"].ToString(), Valid + " " + Disable + " " + "columnType='" + dr["FieldType"].ToString() + "' data-ajax", true, queryValues, true) + "</td>");


                        }
                        else
                        {
                            string FilterColumn = dr["FilterColumn"].ToString();
                            string FilterColumnValue = "";

                            if (!string.IsNullOrEmpty(dr["DefaultValueFromQString"].ToString()))
                            {
                                Uri uri = new Uri(pageLink);
                                FilterColumnValue = HttpUtility.ParseQueryString(uri.Query).Get(dr["DefaultValueFromQString"].ToString());
                            }

                            // For Translation ---------------
                            DataTable dtdata = getDataTableFromQuery("select [" + dr["CheckFieldName"].ToString() + "], [" + dr["ReturnIDFieldName"].ToString() + "]  from " + dr["LinkedTable"].ToString() + " " + (!string.IsNullOrEmpty(FilterColumn) && !string.IsNullOrEmpty(FilterColumnValue) ? " WHERE [" + FilterColumn + "] = " + checkNull(FilterColumnValue) : ""));
                            if (getDataTableFromQuery("select Langselected from userlanguage where empid='" + HttpContext.Current.Session["empid"].ToString() + "'").Rows[0][0].ToString() != "E")
                            {
                                if (dr["reqTrans"].ToString() == "True")
                                {
                                    dtdata = getDataTableFromQuery("select [" + dr["CheckFieldName"].ToString() + "], ISNULL((select top 1 turkish from Translation where term= " + dr["ReturnIDFieldName"].ToString() + ")," + dr["ReturnIDFieldName"].ToString() + ")   from " + dr["LinkedTable"].ToString() + " " + (!string.IsNullOrEmpty(FilterColumn) && !string.IsNullOrEmpty(FilterColumnValue) ? " WHERE [" + FilterColumn + "] = " + checkNull(queryValues) : ""));
                                    boxhtml.Append(createSpanfromselectDD(dtdata, dr["FieldName"].ToString(), Valid + " " + Disable + " " + "columnType='" + dr["FieldType"].ToString() + "' data-ajax", true, queryValues, true) + "</td>");
                                }

                                else
                                    boxhtml.Append(createSpanfromselectDD(dtdata, dr["FieldName"].ToString(), Valid + " " + Disable + " " + "columnType='" + dr["FieldType"].ToString() + "' data-ajax", true, queryValues, true) + "</td>");
                            }
                            else
                                boxhtml.Append(createSpanfromselectDD(dtdata, dr["FieldName"].ToString(), Valid + " " + Disable + " " + "columnType='" + dr["FieldType"].ToString() + "' data-ajax", true, queryValues, true) + "</td>");
                        }
                    }
                    else if (dr["FieldType"].ToString() == "TXT") // TextBox Field
                    {
                        boxhtml.Append("<span type=\"text\" data-ajax columnType='" + dr["FieldType"].ToString() + "' " + Valid + " " + Disable + " id=\"" + dr["FieldName"].ToString() + "\"  maxlength='" + dr["FieldLength"].ToString() + "' value=\"" + queryValues + "\">" + queryValues + "</span></div> </td>");
                    }
                    else if (dr["FieldType"].ToString() == "TextArea") // TextArea Field
                    {
                        boxhtml.Append("<div>" + queryValues + "</div></td>");
                    }
                    else if (dr["FieldType"].ToString() == "TextEditor") // Editor Field
                    {
                        // boxhtml.Append("<textarea readonly data-ajax columnType='" + dr["FieldType"].ToString() + "' " + Valid + " " + Disable + " id=\"" + dr["FieldName"].ToString() + "\"  class=\"form-control\" >" + queryValues + "</textarea></td>");
                        //   boxhtml.Append("<div columnType='" + dr["FieldType"].ToString() + "' " + Valid + " " + Disable + " id=\"" + dr["FieldName"].ToString() + "\"  maxlength='" + dr["FieldLength"].ToString() + "'>" + ReplaceHTMLattributesfromString(queryValues) + "</div></div> </td>");
                        boxhtml.Append("<div><textarea readonly disabled data-ajax columnType='" + dr["FieldType"].ToString() + "' id=\"" + dr["FieldName"].ToString() + "\">" + queryValues + "</textarea></div></td>");
                        boxhtml.Append(@"<script type='text/javascript'>
                                               var editor1 = new nicEditor({
                                                                  iconsPath: BasePath + 'library/Images/nicEditorIcons.gif',
                                                                  maxHeight: 300,
                                                                  fullPanel : false              
                                                              })
                                                                .panelInstance('" + dr["FieldName"].ToString() + "'); $('#" + randomID + " [unselectable]').first().css('width', '100%');$('.nicEdit-main').css('height', '150px');$('.nicEdit-main').css('width', '100%');$('.nicEdit-main').parent().css('width', '100%');$('.nicEdit-main ').css('height', '150px');$('#" + dr["FieldName"].ToString() + "').attr('contenteditable', 'false');</script>");
                    }
                    else if (dr["FieldType"].ToString() == "PopUp") // PopUp Field
                    {
                        boxhtml.Append("<span type=\"text\" readonly id=\"txt" + dr["FieldName"].ToString() + "\"    value=\"" + queryTxt + "\">" + queryTxt + "</span></div> ");
                        boxhtml.Append("<input type=\"hidden\" data-ajax columnType='" + dr["FieldType"].ToString() + "' " + Valid + " id=\"" + dr["FieldName"].ToString() + "\"    value=\"" + queryValues + "\"/></div> </td>");
                    }
                    else if (dr["FieldType"].ToString() == "Hidden")
                    {
                        boxhtml.Append("<input type=\"hidden\" data-ajax columnType='" + dr["FieldType"].ToString() + "' id=\"" + dr["FieldName"].ToString() + "\" " + (string.IsNullOrEmpty(dr["DefaultValueFromQString"].ToString()) ? "" : "QString") + "  value=\"" + (string.IsNullOrEmpty(dr["DefaultValueFromQString"].ToString()) ? dr["DefaultValue"].ToString() : dr["DefaultValueFromQString"].ToString()) + "\"/>");
                    }

                    if (columCount == totalColSpan && dr["FieldType"].ToString() != "Hidden")
                        boxhtml.Append("</tr>");

                }
                boxhtml.Append("</table>");
                //  boxhtml.Append("</div>");
                //  boxhtml.Append("<div class=\"modal-footer\">"
                //     + " <input class=\"btn green\" type=\"button\" lang=\"Save\" id=\"btnSave\" ClickSave" + tempid + " ClickID=\"" + FormID + "\" value=\"" + getTranslatedValue("Save") + "\" /> "
                //    + " <input class=\"btn btn-outline dark\" data-dismiss=\"modal\" lang=\"Close\" type=\"button\" id=\"Button2\" value=\"Close\" />"
                //    + " </div>");

                //                boxhtml.Append(@"<script type='text/javascript'>
                //                                    jq162('.date-picker').datepicker({
                //                                        changeMonth: true,
                //                                        changeYear: true,
                //                                        dateFormat: 'dd M yy'
                //                                    });
                //                                </script>");

            }

            return boxhtml.ToString();
        }

        public string ReplaceHTMLattributesfromString(string HTMLvalue)
        {
            HTMLvalue.Replace("&lt;BR&gt;", "");
            HTMLvalue.Replace("<STRONG>", "<b>");
            HTMLvalue.Replace("</STRONG>", "</b>");

            return HTMLvalue;
        }

        private string GetFieldTemplateView(string tempid, string FormID, string pageLink) //dashboard dynamic fields
        {

            StringBuilder boxhtml = new StringBuilder();
            string queryValues = "", queryTxt = "", Valid = "", Disable = "", PopUpImg = "";
            int columCount = 0;
            DataTable dt = GetFieldTemplate(tempid);
            DataTable dtInfo = new DataTable();
            int totalColSpan = Convert.ToInt32(dt.Rows[0]["TotalColSpan"]);
            double tdPercent = 100 / totalColSpan;
            if (!string.IsNullOrEmpty(FormID))
            {
                dtInfo = GetFormInfo(dt, FormID);
            }

            if (dt.Rows.Count > 0)
            {
                //  boxhtml.Append("<div class=\"modal-body\" style=\"padding: initial;\">");
                boxhtml.Append("<table style=\"width: 100%\" class=\"cellgap\">");

                foreach (DataRow dr in dt.Rows)
                {
                    int colspan = 1; int.TryParse(dr["colspan"].ToString(), out colspan);
                    int randomID = new Random().Next();

                    if (dtInfo.Rows.Count > 0)
                    {
                        queryValues = dtInfo.Rows[0][dr["FieldName"].ToString()].ToString();

                        if (dr["FieldType"].ToString() == "PopUp")
                        {
                            queryTxt = dtInfo.Rows[0]["TXT" + dr["FieldName"].ToString()].ToString();
                        }
                        else
                        {
                            queryTxt = "";
                        }
                    }

                    if (dr["Mandetory"].ToString() == "Y")
                        Valid = "valid-ajax='vld" + dr["FieldName"].ToString() + randomID + "'";
                    else
                        Valid = "";

                    if (dr["IsDisable"].ToString() == "True")
                        Disable = "disabled";
                    else
                        Disable = "";


                    if (columCount == totalColSpan && dr["FieldType"].ToString() != "Hidden")
                    {
                        boxhtml.Append("<tr>");
                        columCount = 0;
                    }
                    else if (dr["FieldType"].ToString() == "TextEditor")
                    {
                        boxhtml.Append("</tr><tr>");
                        columCount = totalColSpan;
                    }
                    //else
                    //{
                    columCount = columCount + colspan;
                    //}

                    // set popup image
                    if (dr["FieldType"].ToString() == "PopUp" ||dr["FieldType"].ToString() == "DDWPopUp")
                    {
                        PopUpImg = "<img style='cursor: hand' onclick=\"PopUpWindow('" + dr["PopUpKey"].ToString() + "', '" + dr["FieldName"].ToString() + "');\" alt='Search' src='../../images/search.png'>";
                        //if (dr["PopUpKey"].ToString() == "folder")
                        //{
                        //    boxhtml.Append("<script type='text/javascript'>" +
                        //               "var opfl;" +
                        //               "function opnfldr(val) {" +
                        //               "opfl = val;" +
                        //               "if (typeof opfl != 'undefined' && opfl != '') {" +
                        //                  "var aryRetValue = opfl.split('}{');" +
                        //                  "$('#" + dr["FieldName"].ToString() + "').val(aryRetValue[0].toString());" +
                        //                  "$('#txt" + dr["FieldName"].ToString() + "').val(aryRetValue[1].toString());" +
                        //                 "}" +
                        //               "else" +
                        //                 " { return; } " +
                        //               " }" +
                        //             "</script>");
                        //}

                        if (dr["PopUpKey"].ToString() == "folder")
                        {
                            boxhtml.Append("<script type='text/javascript'>" +
                                       "var opfl;" +
                                       "function circular(val) {" +
                                       "opfl = val;" +
                                       "if (typeof opfl != 'undefined' && opfl != '') {" +
                                          "$('#" + dr["FieldName"].ToString() + "').val(val[0].toString());" +
                                          "$('#txt" + dr["FieldName"].ToString() + "').val(val[1].toString());" +
                                         "}" +
                                       "else" +
                                         " { return; } " +
                                       " }" +
                                     "</script>");
                        }
                    }
                    else
                        PopUpImg = "";


                    if (dr["FieldType"].ToString() != "Hidden")
                        // Label 
                        boxhtml.Append("<td style='width: " + (tdPercent * colspan) + "%' colspan=\"" + colspan + "\" id='td" + dr["FieldName"].ToString() + "' > <div class=\"form-group\">   <label class=\"control-label\" lang=\"" + dr["DisplayColumnName"].ToString().Replace(" ", "_") + "\">" + dr["DisplayColumnName"].ToString() + "</label> " + PopUpImg + " <span id='vld" + dr["FieldName"].ToString() + randomID + "' style='display:none; color:red'>*</span>");



                    if (dr["FieldType"].ToString() == "D") // Date Field
                    {
                        boxhtml.Append("<input type=\"text\" readonly data-ajax columnType='" + dr["FieldType"].ToString() + "' " + Valid + " " + Disable + "  id=\"" + dr["FieldName"].ToString() + "\"  class=\"form-control form-control-inline date-picker\" value=\"" + queryValues + "\" /></div> </td>");
                    }
                    else if (dr["FieldType"].ToString() == "A") // Amount Field
                    {
                        boxhtml.Append("<input type=\"text\" data-ajax columnType='" + dr["FieldType"].ToString() + "' " + Valid + " " + Disable + " id=\"" + dr["FieldName"].ToString() + "\"  class=\"form-control\" onkeypress=\"return blockNonNumbers(this, event, false, true);\" onfocus=\"removeCommasR(this.value,this.id);\" onblur=\"CurrencyFormattedR(this.value,this.id)\" onkeyup=\"zeroPaddingR(event.keyCode,this.id)\" maxlength='" + dr["FieldLength"].ToString() + "' value=\"" + queryValues + "\"/></div> </td>");
                    }
                    else if (dr["FieldType"].ToString() == "I") // Int Field
                    {
                        boxhtml.Append("<input type=\"text\" data-ajax columnType='" + dr["FieldType"].ToString() + "' " + Valid + " " + Disable + " id=\"" + dr["FieldName"].ToString() + "\"  class=\"form-control\" onkeypress=\"return blockNonNumbers(this, event, false, true);\" maxlength='" + dr["FieldLength"].ToString() + "' value=\"" + queryValues + "\"  /></div> </td>");
                    }
                    else if (dr["FieldType"].ToString() == "DD")// Dropdown Field
                    {
                        if (dr["Query"].ToString().Trim() != "")
                        {
                            // boxhtml.Append(createDropDown_selected(getDataTableFromQuery(dr["Query"].ToString()), dr["FieldName"].ToString(), Valid + " " + Disable + " " + "columnType='" + dr["FieldType"].ToString() + "' data-ajax", true, queryValues, true) + "</td>");

                            DataTable dtdata = getDataTableFromQuery(dr["Query"].ToString());
                            if (getDataTableFromQuery("select Langselected from userlanguage where empid='" + HttpContext.Current.Session["empid"].ToString() + "'").Rows[0][0].ToString() != "E")
                            {
                                if (dr["reqTrans"].ToString() == "True")
                                {
                                    // dtdata = getDataTableFromQuery("select [" + dr["CheckFieldName"].ToString() + "], ISNULL((select top 1 turkish from Translation where term= " + dr["ReturnIDFieldName"].ToString() + ")," + dr["ReturnIDFieldName"].ToString() + ")   from " + dr["LinkedTable"].ToString() + " " + (!string.IsNullOrEmpty(FilterColumn) && !string.IsNullOrEmpty(FilterColumnValue) ? " WHERE [" + FilterColumn + "] = " + checkNull(FilterColumnValue) : ""));
                                    boxhtml.Append(createDropDown_selected(dtdata, dr["FieldName"].ToString(), Valid + " " + Disable + " " + "columnType='" + dr["FieldType"].ToString() + "' data-ajax", true, queryValues, true) + "</td>");
                                }

                                else
                                    boxhtml.Append(createDropDownW(dtdata, dr["FieldName"].ToString(), Valid + " " + Disable + " " + "columnType='" + dr["FieldType"].ToString() + "' data-ajax", true, queryValues, true) + "</td>");
                            }
                            else
                                boxhtml.Append(createDropDownW(dtdata, dr["FieldName"].ToString(), Valid + " " + Disable + " " + "columnType='" + dr["FieldType"].ToString() + "' data-ajax", true, queryValues, true) + "</td>");


                        }
                        else
                        {
                            string FilterColumn = dr["FilterColumn"].ToString();
                            string FilterColumnValue = "";

                            if (!string.IsNullOrEmpty(dr["DefaultValueFromQString"].ToString()))
                            {
                                Uri uri = new Uri(pageLink);
                                FilterColumnValue = HttpUtility.ParseQueryString(uri.Query).Get(dr["DefaultValueFromQString"].ToString());
                            }

                            // For Translation ---------------
                            DataTable dtdata = getDataTableFromQuery("select [" + dr["CheckFieldName"].ToString() + "], [" + dr["ReturnIDFieldName"].ToString() + "]  from " + dr["LinkedTable"].ToString() + " " + (!string.IsNullOrEmpty(FilterColumn) && !string.IsNullOrEmpty(FilterColumnValue) ? " WHERE [" + FilterColumn + "] = " + checkNull(FilterColumnValue) : ""));
                            if (getDataTableFromQuery("select Langselected from userlanguage where empid='" + HttpContext.Current.Session["empid"].ToString() + "'").Rows[0][0].ToString() != "E")
                            {
                                if (dr["reqTrans"].ToString() == "True")
                                {
                                    dtdata = getDataTableFromQuery("select [" + dr["CheckFieldName"].ToString() + "], ISNULL((select top 1 turkish from Translation where term= " + dr["ReturnIDFieldName"].ToString() + ")," + dr["ReturnIDFieldName"].ToString() + ")   from " + dr["LinkedTable"].ToString() + " " + (!string.IsNullOrEmpty(FilterColumn) && !string.IsNullOrEmpty(FilterColumnValue) ? " WHERE [" + FilterColumn + "] = " + checkNull(FilterColumnValue) : ""));
                                    boxhtml.Append(createDropDown_selected(dtdata, dr["FieldName"].ToString(), Valid + " " + Disable + " " + "columnType='" + dr["FieldType"].ToString() + "' data-ajax", true, queryValues, true) + "</td>");
                                }

                                else
                                    boxhtml.Append(createDropDownW(dtdata, dr["FieldName"].ToString(), Valid + " " + Disable + " " + "columnType='" + dr["FieldType"].ToString() + "' data-ajax", true, queryValues, true) + "</td>");
                            }
                            else
                                boxhtml.Append(createDropDownW(dtdata, dr["FieldName"].ToString(), Valid + " " + Disable + " " + "columnType='" + dr["FieldType"].ToString() + "' data-ajax", true, queryValues, true) + "</td>");
                        }
                    }
                    else if (dr["FieldType"].ToString() == "TXT") // TextBox Field
                    {
                        boxhtml.Append("<input type=\"text\" data-ajax columnType='" + dr["FieldType"].ToString() + "' " + Valid + " " + Disable + " id=\"" + dr["FieldName"].ToString() + "\"  class=\"form-control\"  maxlength='" + dr["FieldLength"].ToString() + "' value=\"" + queryValues + "\"/></div> </td>");
                    }
                    else if (dr["FieldType"].ToString() == "TextArea") // TextArea Field
                    {
                        boxhtml.Append("<textarea data-ajax columnType='" + dr["FieldType"].ToString() + "' " + Valid + " " + Disable + " id=\"" + dr["FieldName"].ToString() + "\"  class=\"form-control\" >" + queryValues + "</textarea></td>");
                    }
                    else if (dr["FieldType"].ToString() == "TextEditor") // Editor Field
                    {
                        boxhtml.Append("<div><textarea data-ajax columnType='" + dr["FieldType"].ToString() + "' " + Valid + " " + Disable + " id=\"" + dr["FieldName"].ToString() + "\"  class=\"form-control\" >" + queryValues + "</textarea></div></td>");
                        boxhtml.Append(@"<script type='text/javascript'>
                                                var editor1 = new nicEditor({
                                                                  iconsPath: BasePath + 'library/Images/nicEditorIcons.gif',
                                                                  maxHeight: 300
                                                              }).panelInstance('" + dr["FieldName"].ToString() + "'); $('#" + randomID + " [unselectable]').first().css('width', '100%');$('.nicEdit-main').css('text-align', 'Left');$('.nicEdit-main').css('height', '150px');$('.nicEdit-main').css('width', '100%');$('.nicEdit-main').parent().css('width', '100%');$('.nicEdit-main ').css('height', '150px');</script>");
                    }
                    else if (dr["FieldType"].ToString() == "PopUp") // PopUp Field
                    {
                        boxhtml.Append("<input type=\"text\" readonly id=\"txt" + dr["FieldName"].ToString() + "\"  class=\"form-control\"  value=\"" + queryTxt + "\"/></div> ");
                        boxhtml.Append("<input type=\"hidden\" data-ajax columnType='" + dr["FieldType"].ToString() + "' " + Valid + " id=\"" + dr["FieldName"].ToString() + "\"  class=\"form-control\"  value=\"" + queryValues + "\"/></div> </td>");
                    }

                    else if (dr["FieldType"].ToString() == "DDWPopUp") //Drop Down PopUp Field
                    {
                        if (dr["Query"].ToString().Trim() != "")
                        {
                            DataTable dtdata = getDataTableFromQuery(dr["Query"].ToString());
                            boxhtml.Append(createDropDownW(dtdata, dr["FieldName"].ToString(), Valid + " " + Disable + " " + "columnType='" + dr["FieldType"].ToString() + "' data-ajax", true, queryValues, true) + "</td>");
                        }
                        boxhtml.Append("<input type=\"text\" readonly id=\"txt" + dr["FieldName"].ToString() + "\"  class=\"form-control\"  value=\"" + queryTxt + "\" style=\"display:none\"/></div> ");
                        boxhtml.Append("<input type=\"hidden\" columnType='" + dr["FieldType"].ToString() + "' " + Valid + " id=\"" + dr["FieldName"].ToString() + "-chosen\"  class=\"form-control\"  value=\"" + queryValues + "\"/></div> </td>");
                    }
                    else if (dr["FieldType"].ToString() == "DDWChk")
                    {
                        boxhtml.Append("<div class='mc-field-group' style='width: 100%;'>" +
                                         "<div class='multiselect' id='ruleB'>" +
                                            "<div class='selectBox' onclick='showCheckboxes(\"chk_" + dr["FieldName"].ToString() + "\")'>" +
                                         "<select  data-ajax id=\"" + dr["FieldName"].ToString() + "\" class='form-control'>" +
                                         "<option></option>" +
                                          "</select>" +
                                          "<div class='overSelect'>" +
                                          "</div>" +
                                          "</div>" +
                                          "<div id='chk_" + dr["FieldName"].ToString() + "' style='display:none'> ");


                        if (dr["Query"].ToString().Trim() != "")
                        {
                            DataTable dtdata1 = getDataTableFromQuery(dr["Query"].ToString());
                            foreach (DataRow dr1 in dtdata1.Rows)
                                boxhtml.Append("<p for='" + dr1["ID"] + "' class='cls_check' style=\"height: 2%;\"><input value=\'" + dr1["Value"] + "\' class='check' type='checkbox'   id='" + dr1["ID"] + "' onclick=\"getChecks(this,'" + dr["FieldName"].ToString() + "')\" />    " + dr1["Value"] + "</p>");
                        }
                        else
                        {
                            string FilterColumn = dr["FilterColumn"].ToString();
                            string FilterColumnValue = "";


                            // For Translation ---------------
                            DataTable dtdata1 = getDataTableFromQuery("select [" + dr["CheckFieldName"].ToString() + "] ID, [" + dr["ReturnIDFieldName"].ToString() + "] Value from " + dr["LinkedTable"].ToString() + " " + (!string.IsNullOrEmpty(FilterColumn) && !string.IsNullOrEmpty(FilterColumnValue) ? " WHERE [" + FilterColumn + "] = " + checkNull(FilterColumnValue) : ""));

                            foreach (DataRow dr1 in dtdata1.Rows)
                                boxhtml.Append("<p for='" + dr1["ID"] + "' class='cls_check' style=\"height: 2%;\"><input value=\'" + dr1["Value"] + "\' class='check' type='checkbox'   id='" + dr1["ID"] + "' onclick=\"getChecks(this)\" />    " + dr1["Value"] + "</p>");
                        }
                        boxhtml.Append(@"</div>
                            </div>
                        </div>");
                    }
                    else if (dr["FieldType"].ToString() == "Hidden")
                    {
                        boxhtml.Append("<input type=\"hidden\" data-ajax columnType='" + dr["FieldType"].ToString() + "' id=\"" + dr["FieldName"].ToString() + "\" " + (string.IsNullOrEmpty(dr["DefaultValueFromQString"].ToString()) ? "" : "QString") + "  value=\"" + (string.IsNullOrEmpty(dr["DefaultValueFromQString"].ToString()) ? dr["DefaultValue"].ToString() : dr["DefaultValueFromQString"].ToString()) + "\"/>");
                    }

                    if (columCount == totalColSpan && dr["FieldType"].ToString() != "Hidden")
                        boxhtml.Append("</tr>");

                }
                boxhtml.Append("</table>");
                //boxhtml.Append("</div>");
                //boxhtml.Append("<div class=\"modal-footer\">"
                //   + " <input class=\"btn green\" type=\"button\" lang=\"Save\" id=\"btnSave\" ClickSave" + tempid + " ClickID=\"" + FormID + "\" value=\"" + getTranslatedValue("Save") + "\" /> "
                //   + " <input class=\"btn btn-outline dark\" data-dismiss=\"modal\" lang=\"Close\" type=\"button\" id=\"Button2\" value=\"Close\" />"
                //   + " </div>");

                boxhtml.Append(@"<script type='text/javascript'>
                                    jq162('.date-picker').datepicker({
                                        changeMonth: true,
                                        changeYear: true,
                                        dateFormat: 'dd M yy'
                                    });
                                </script>");
                //formultiselect
                boxhtml.Append(@"<script type='text/javascript'>
                                    var expanded = false;
                                    function showCheckboxes(idchk) {
                                    $('#me').css({ 'background-color': 'white' });
                                    if (document.getElementById(idchk) != null) {
                                    var checkboxes = document.getElementById(idchk);
                                    if (!expanded) {
                                    var styles = { 'overflow-y': 'auto', 'background-Color': 'white', 'z-index': '1', 'display': 'block', 'position': 'absolute', 'width': '31%', 'max-height': '200px', 'border': '1px solid black', 'padding-left': '10px' }
                                    $('#' + idchk).css(styles);
                                    expanded = true;
                                     }
                                      else {
                                     checkboxes.style.display = 'none';
                                    expanded = false;
                                     }
                                   }
                                  }
                              </script>");


            }

            return boxhtml.ToString();
        }

        #endregion

        #endregion

        public ArrayList customSave(DataTable dtData, string pageLink)
        {
            string action = dtData.Select("key = 'SaveType'")[0]["data"].ToString();
            ArrayList retAry = new ArrayList();
            string pageName = System.IO.Path.GetFileName(new Uri(pageLink).LocalPath);
            if (action == "GetDynamicGridHeader")
            {
                string TempID = dtData.Select("key = 'TempID'")[0]["data"].ToString();

                retAry.Add("");
                retAry.Add(getHeaderfortable(TempID));
                retAry.Add(new DataTable());
            }
            else if (action == "GetInspFieldTemplate")
            {
                string TempID = dtData.Select("key = 'TempID'")[0]["data"].ToString();
                string FormID = dtData.Select("key = 'FormID'")[0]["data"].ToString();


                retAry.Add("");
                retAry.Add(GetFieldTemplate(TempID, FormID, pageLink));
                retAry.Add(new DataTable());

                ArrayList ex1 = new ArrayList();
                ex1.Add("JsonHideShow");
                ex1.Add(GetJsonForHideShowFields(TempID));

                retAry.Add(ex1);

            }
            else if (action == "SaveDynamicFields")
            {
                string TempID = dtData.Select("key = 'TempID'")[0]["data"].ToString();
                string formID = dtData.Select("key = 'formID'")[0]["data"].ToString();

                InsertDynamicFields(TempID, dtData, formID);

                retAry.Add("Save Succesfully.");
                retAry.Add("");
                retAry.Add(new DataTable());
            }
            else if (action == "DeleteDynamicFields")
            {
                string TempID = dtData.Select("key = 'TempID'")[0]["data"].ToString();
                string formID = dtData.Select("key = 'formID'")[0]["data"].ToString();

                DeleteDynamicFields(TempID, formID, pageName);

                retAry.Add("Delete Succesfully.");
                retAry.Add("");
                retAry.Add(new DataTable());
            }
            else if (action == "GetDynamicTree")
            {
                string TempID = dtData.Select("key = 'TempID'")[0]["data"].ToString();

                retAry.Add("");
                retAry.Add(getDynamicTree(TempID));
                retAry.Add(new DataTable());
            }
            else if (action == "getBasicProp")
            {
                string TempID = dtData.Select("key = 'TempID'")[0]["data"].ToString();

                retAry.Add("");
                retAry.Add("");
                retAry.Add(getBasicProp(TempID));
            }
            else if (action == "showDynamicview")
            {
                string TempID = dtData.Select("key = 'TempID'")[0]["data"].ToString();
                string FormID = dtData.Select("key = 'FormID'")[0]["data"].ToString();


                retAry.Add("");
                retAry.Add(getDynamicVIEWONLY(TempID, FormID, pageLink));
                retAry.Add(new DataTable());

                ArrayList ex1 = new ArrayList();
                ex1.Add("JsonHideShow");
                ex1.Add(GetJsonForHideShowFields(TempID));

                retAry.Add(ex1);

            }

            else if (action == "GetInspFieldTemplateView")
            {
                string TempID = dtData.Select("key = 'TempID'")[0]["data"].ToString();
                string FormID = dtData.Select("key = 'FormID'")[0]["data"].ToString();


                retAry.Add("");
                retAry.Add(GetFieldTemplateView(TempID, FormID, pageLink));
                retAry.Add(new DataTable());

                ArrayList ex1 = new ArrayList();
                ex1.Add("JsonHideShow");
                ex1.Add(GetJsonForHideShowFields(TempID));

                retAry.Add(ex1);

            }
            return retAry;
        }
    }


}
