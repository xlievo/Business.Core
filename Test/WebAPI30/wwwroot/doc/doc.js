var fileform = "root[input][f]";

function renderSize(value) {
    if (null == value || value == '') {
        return "0 Bytes";
    }
    var unitArr = new Array("Bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB");
    var index = 0;
    var srcsize = parseFloat(value);
    index = Math.floor(Math.log(srcsize) / Math.log(1024));
    var size = srcsize / Math.pow(1024, index);
    size = size.toFixed(2);
    return size + unitArr[index];
}

var $each = function (obj, callback) {
    if (!obj || typeof obj !== "object") return;
    var i;
    if (Array.isArray(obj) || (typeof obj.length === 'number' && obj.length > 0 && (obj.length - 1) in obj)) {
        for (i = 0; i < obj.length; i++) {
            if (callback(i, obj[i]) === false) return;
        }
    }
    else {
        if (Object.keys) {
            var keys = Object.keys(obj);
            for (i = 0; i < keys.length; i++) {
                if (callback(keys[i], obj[keys[i]]) === false) return;
            }
        }
        else {
            for (i in obj) {
                if (!obj.hasOwnProperty(i)) continue;
                if (callback(i, obj[i]) === false) return;
            }
        }
    }
};

var $trigger = function (el, event) {
    var e = document.createEvent('HTMLEvents');
    e.initEvent(event, true, true);
    el.dispatchEvent(e);
};

var ajax = {};
ajax.x = function () {
    if (typeof XMLHttpRequest !== 'undefined') {
        return new XMLHttpRequest();
    }
    var versions = [
        "MSXML2.XmlHttp.6.0",
        "MSXML2.XmlHttp.5.0",
        "MSXML2.XmlHttp.4.0",
        "MSXML2.XmlHttp.3.0",
        "MSXML2.XmlHttp.2.0",
        "Microsoft.XmlHttp"
    ];

    var xhr;
    for (var i = 0; i < versions.length; i++) {
        try {
            xhr = new ActiveXObject(versions[i]);
            break;
        } catch (e) {
        }
    }
    return xhr;
};

ajax.send = function (url, callback, failed, method, data, async, contentType = 'application/x-www-form-urlencoded') {
    if (async === undefined) {
        async = true;
    }
    var x = ajax.x();
    x.open(method, url, async);
    x.onreadystatechange = function () {
        if (x.readyState !== 4) return;

        if (x.status >= 200 && x.status < 300) {
            callback(x.responseText)
        }
        else {
            failed({
                status: x.status,
                statusText: x.statusText,
                responseText: x.responseText
            })
        }
    };
    if (method == 'POST') {
        if (null !== contentType && '' !== contentType) {
            x.setRequestHeader('Content-type', contentType);
        }
    }
    x.send(data);
};

ajax.get = function (url, data, callback, failed, async) {
    var query = [];
    for (var key in data) {
        query.push(encodeURIComponent(key) + '=' + encodeURIComponent(data[key]));
    }
    ajax.send(url + (query.length ? '?' + query.join('&') : ''), callback, failed, 'GET', null, async)
};

ajax.post = function (url, data, callback, failed, async) {
    var query = [];
    for (var key in data) {
        query.push(encodeURIComponent(key) + '=' + encodeURIComponent(data[key]));
    }
    ajax.send(url, callback, failed, 'POST', query.join('&'), async)
};

ajax.postForm = function (url, data, callback, failed, async) {
    ajax.send(url, callback, failed, 'POST', data, async, null)
};

//const choices = new Choices("choices-single-no-search");
// Set the default CSS theme and icon library globally
JSONEditor.defaults.theme = 'bootstrap3';
JSONEditor.defaults.iconlib = 'bootstrap3';
JSONEditor.defaults.options.disable_properties = true;
JSONEditor.defaults.options.required_by_default = true;
JSONEditor.defaults.options.disable_edit_json = true;
//JSONEditor.defaults.options.disable_collapse = true;

var Theme = JSONEditor.defaults.themes.bootstrap3;
JSONEditor.defaults.themes.bootstrap3 = function () {
    var theme = new Theme();

    theme.getButton = function (text, icon, title) {
        var el = document.createElement('button');
        el.classList.add('btn', 'btn-default');
        el.type = 'button';
        this.setButtonText(el, text, icon, title);
        el.setAttribute('tag', 'collapse');
        if (el.title === 'Collapse') {
            el.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();

                var h5 = null;
                var row = el.parentNode.parentNode.parentNode.parentNode.nextSibling;
                var description = el.parentNode.parentNode.nextSibling;
                if (null != description && description.tagName == "P") {
                    var h5s = description.querySelectorAll("h5");
                    if (0 < h5s.length) {
                        h5 = h5s[h5s.length - 1];
                    }
                }

                if (el.title === 'Collapse') {
                    if (null != h5) {
                        h5.style.marginBottom = null == row ? '2px' : '6px';
                    }
                } else {
                    if (null != h5) {
                        h5.style.marginBottom = '15px';
                    }
                }
            });
        }
        return el;
    };
    theme.setButtonText = function (button, text, icon, title) {
        // Clear previous contents. https://jsperf.com/innerhtml-vs-removechild/37
        while (button.firstChild) {
            button.removeChild(button.firstChild);
        }
        if (icon) {
            button.appendChild(icon);
            text = ' ' + text;
        }
        var spanEl = document.createElement('span');
        spanEl.appendChild(document.createTextNode(text));
        button.appendChild(spanEl);
        if (title) button.setAttribute('title', title);
    };

    theme.getGridRow = function () {
        var el = document.createElement('div');
        el.classList.add('row');
        el.style.paddingTop = el.style.paddingBottom = el.style.marginTop = el.style.marginBottom = '0px';
        el.setAttribute('tag', 'row');
        el.style.marginTop = '8px';
        el.style.marginBottom = '8px';
        return el;
    };

    theme.getHeader = function (text) {
        var el = document.createElement('h4');
        el.style.paddingTop = el.style.paddingBottom = el.style.marginTop = el.style.marginBottom = '0px';
        el.setAttribute('tag', 'h4');
        if (typeof text === "string") {
            el.textContent = text;
        }
        else {
            el.appendChild(text);
        }
        return el;
    };

    theme.getFormControl = function (label, input, description, infoText) {
        var group = document.createElement('div');
        group.style.paddingTop = group.style.paddingBottom = group.style.marginTop = group.style.marginBottom = '0px';

        if (label && input.type === 'checkbox') {
            group.classList.add('checkbox');
            label.appendChild(input);
            label.style.fontSize = '14px';
            if (infoText) group.appendChild(infoText);
            group.appendChild(label);
            input.style.position = 'relative';
            input.style.cssFloat = 'left';
        }
        else {
            group.classList.add('form-group');
            if (label) {
                label.classList.add('control-label');
                label.setAttribute('tag', 'label');
                group.appendChild(label);
                label.style.margin = '0px';
            }

            if (infoText) group.appendChild(infoText);
            group.appendChild(input);
        }

        if (description) {
            //description.style.marginTop = '0px';
            //description.style.marginBottom = '0px';
            //description.setAttribute('tag', 'description');
            group.appendChild(description);
        }

        return group;
    };

    theme.getIndentedPanel = function () {
        var el = document.createElement('div');
        el.classList.add('well', 'well-sm');
        el.style.paddingTop = el.style.paddingBottom = el.style.marginTop = el.style.marginBottom = '0px';
        el.style.marginTop = '8px';
        el.setAttribute('tag', 'panel');
        return el;
    };

    theme.getSelectInput = function (options) {
        var select = document.createElement('select');
        if (options) this.setSelectOptions(select, options);
        select.classList.add('form-control');
        select.setAttribute('tag', 'input');
        select.style.marginTop = '5px';
        return select;
    };

    theme.getTextareaInput = function () {
        var el = document.createElement('textarea');
        el.classList.add('form-control');
        el.setAttribute('tag', 'input');
        el.style.marginTop = '5px';
        return el;
    };

    theme.getFormInputField = function (type) {
        var el = document.createElement('input');
        el.setAttribute('type', type);
        el.setAttribute('tag', 'input');
        el.style.marginTop = '5px';
        if (type !== "checkbox") {
            el.classList.add("form-control");
        }
        if (el.type == "file") {
            el.style.paddingTop = '4px';
        }
        return el;
    };

    theme.getDescription = function (text) {
        var el = document.createElement('p');
        el.style.paddingTop = el.style.paddingBottom = el.style.marginTop = el.style.marginBottom = '0px';
        el.style.marginTop = '8px';
        el.style.marginBottom = '2px';
        el.setAttribute('tag', 'description');
        if (window.DOMPurify) el.innerHTML = window.DOMPurify.sanitize(text);
        else el.textContent = this.cleanText(text);
        return el;
    };

    theme.getFormInputDescription = function (text) {
        var el = document.createElement('p');
        el.style.paddingTop = el.style.paddingBottom = el.style.marginTop = el.style.marginBottom = '0px';
        el.style.marginBottom = '2px';
        el.style.marginTop = '5px';
        el.setAttribute('tag', 'description');
        el.classList.add('help-block');
        if (window.DOMPurify) el.innerHTML = window.DOMPurify.sanitize(text);
        else el.textContent = this.cleanText(text);
        return el;
    };

    return theme
};

JSONEditor.defaults.editors.object = JSONEditor.defaults.editors.object.extend({
    build: function () {
        var self = this;

        var isCategoriesFormat = (this.format === 'categories');
        this.rows = [];
        this.active_tab = null;

        // If the object should be rendered as a table row
        if (this.options.table_row) {
            this.editor_holder = this.container;
            $each(this.editors, function (key, editor) {
                var holder = self.theme.getTableCell();
                self.editor_holder.appendChild(holder);

                editor.setContainer(holder);
                editor.build();
                editor.postBuild();
                editor.setOptInCheckbox(editor.header);

                if (self.editors[key].options.hidden) {
                    holder.style.display = 'none';
                }
                if (self.editors[key].options.input_width) {
                    holder.style.width = self.editors[key].options.input_width;
                }
            });
        }
        // If the object should be rendered as a table
        else if (this.options.table) {
            // TODO: table display format
            throw "Not supported yet";
        }
        // If the object should be rendered as a div
        else {
            this.header = document.createElement('label');
            this.header.textContent = this.getTitle();
            this.title = this.theme.getHeader(this.header);
            this.container.appendChild(this.title);
            this.container.style.position = 'relative';

            // Edit JSON modal
            this.editjson_holder = this.theme.getModal();
            this.editjson_textarea = this.theme.getTextareaInput();
            this.editjson_textarea.style.marginTop = "0px";
            this.editjson_textarea.style.height = '170px';
            this.editjson_textarea.style.width = '300px';
            this.editjson_textarea.style.display = 'block';
            this.editjson_save = this.getButton('Save', 'save', 'Save');
            this.editjson_save.style.display = 'none';
            this.editjson_save.classList.add('json-editor-btntype-save');
            this.editjson_save.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                self.saveJSON();
            });
            this.editjson_copy = this.getButton('Copy', 'copy', 'Copy');
            this.editjson_copy.classList.add('json-editor-btntype-copy');
            this.editjson_copy.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                self.copyJSON();
            });
            this.editjson_cancel = this.getButton('Cancel', 'cancel', 'Cancel');
            this.editjson_cancel.classList.add('json-editor-btntype-cancel');
            this.editjson_cancel.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                self.hideEditJSON();
            });
            this.editjson_holder.appendChild(this.editjson_textarea);
            this.editjson_holder.appendChild(this.editjson_save);
            this.editjson_holder.appendChild(this.editjson_copy);
            this.editjson_holder.appendChild(this.editjson_cancel);

            // Manage Properties modal
            this.addproperty_holder = this.theme.getModal();
            this.addproperty_list = document.createElement('div');
            this.addproperty_list.style.width = '295px';
            this.addproperty_list.style.maxHeight = '160px';
            this.addproperty_list.style.padding = '5px 0';
            this.addproperty_list.style.overflowY = 'auto';
            this.addproperty_list.style.overflowX = 'hidden';
            this.addproperty_list.style.paddingLeft = '5px';
            this.addproperty_list.setAttribute('class', 'property-selector');
            this.addproperty_add = this.getButton('add', 'add', 'add');
            this.addproperty_add.classList.add('json-editor-btntype-add');
            this.addproperty_input = this.theme.getFormInputField('text');
            this.addproperty_input.setAttribute('placeholder', 'Property name...');
            this.addproperty_input.style.width = '220px';
            this.addproperty_input.style.marginBottom = '0';
            this.addproperty_input.style.display = 'inline-block';
            this.addproperty_add.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                if (self.addproperty_input.value) {
                    if (self.editors[self.addproperty_input.value]) {
                        window.alert('there is already a property with that name');
                        return;
                    }

                    self.addObjectProperty(self.addproperty_input.value);
                    if (self.editors[self.addproperty_input.value]) {
                        self.editors[self.addproperty_input.value].disable();
                    }
                    self.onChange(true);
                }
            });
            this.addproperty_holder.appendChild(this.addproperty_list);
            this.addproperty_holder.appendChild(this.addproperty_input);
            this.addproperty_holder.appendChild(this.addproperty_add);
            var spacer = document.createElement('div');
            spacer.style.clear = 'both';
            this.addproperty_holder.appendChild(spacer);

            // Close properties modal if clicked outside modal
            document.addEventListener('click', function (e) {
                if (!this.addproperty_holder.contains(e.target) && this.adding_property) {
                    e.preventDefault();
                    e.stopPropagation();
                    this.toggleAddProperty();
                }
            }.bind(this));

            // Description
            if (this.schema.description) {
                this.description = this.theme.getDescription(this.schema.description);
                this.container.appendChild(this.description);
            }

            // Validation error placeholder area
            this.error_holder = document.createElement('div');
            this.container.appendChild(this.error_holder);

            // Container for child editor area
            this.editor_holder = this.theme.getIndentedPanel();
            this.container.appendChild(this.editor_holder);

            // Container for rows of child editors
            this.row_container = this.theme.getGridContainer();

            if (isCategoriesFormat) {
                this.tabs_holder = this.theme.getTopTabHolder(this.getValidId(this.schema.title));
                this.tabPanesContainer = this.theme.getTopTabContentHolder(this.tabs_holder);
                this.editor_holder.appendChild(this.tabs_holder);
            }
            else {
                this.tabs_holder = this.theme.getTabHolder(this.getValidId(this.schema.title));
                this.tabPanesContainer = this.theme.getTabContentHolder(this.tabs_holder);
                this.editor_holder.appendChild(this.row_container);
            }

            $each(this.editors, function (key, editor) {
                var aPane = self.theme.getTabContent();
                var holder = self.theme.getGridColumn();
                var isObjOrArray = (editor.schema && (editor.schema.type === 'object' || editor.schema.type === 'array')) ? true : false;
                aPane.isObjOrArray = isObjOrArray;

                if (isCategoriesFormat) {
                    if (isObjOrArray) {
                        var single_row_container = self.theme.getGridContainer();
                        single_row_container.appendChild(holder);
                        aPane.appendChild(single_row_container);
                        self.tabPanesContainer.appendChild(aPane);
                        self.row_container = single_row_container;
                    }
                    else {
                        if (typeof self.row_container_basic === 'undefined') {
                            self.row_container_basic = self.theme.getGridContainer();
                            aPane.appendChild(self.row_container_basic);
                            if (self.tabPanesContainer.childElementCount == 0) {
                                self.tabPanesContainer.appendChild(aPane);
                            }
                            else {
                                self.tabPanesContainer.insertBefore(aPane, self.tabPanesContainer.childNodes[1]);
                            }
                        }
                        self.row_container_basic.appendChild(holder);
                    }

                    self.addRow(editor, self.tabs_holder, aPane);

                    aPane.id = self.getValidId(editor.schema.title); //editor.schema.path//tab_text.textContent

                }
                else {
                    self.row_container.appendChild(holder);
                }

                editor.setContainer(holder);
                editor.build();
                editor.postBuild();
                editor.setOptInCheckbox(editor.header);
            });

            if (this.rows[0]) {
                $trigger(this.rows[0].tab, 'click');
            }

            // Control buttons
            this.title_controls = this.theme.getHeaderButtonHolder();
            this.editjson_controls = this.theme.getHeaderButtonHolder();
            this.addproperty_controls = this.theme.getHeaderButtonHolder();
            this.title.appendChild(this.title_controls);
            this.title.appendChild(this.editjson_controls);
            this.title.appendChild(this.addproperty_controls);

            // Show/Hide button
            this.collapsed = false;
            this.toggle_button = this.getButton('', 'collapse', this.translate('button_collapse'));
            this.toggle_button.classList.add('json-editor-btntype-toggle');
            this.title_controls.appendChild(this.toggle_button);
            this.toggle_button.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                if (self.collapsed) {
                    self.editor_holder.style.display = '';
                    self.collapsed = false;
                    self.setButtonText(self.toggle_button, '', 'collapse', self.translate('button_collapse'));
                }
                else {
                    self.editor_holder.style.display = 'none';
                    self.collapsed = true;
                    self.setButtonText(self.toggle_button, '', 'expand', self.translate('button_expand'));
                }
            });

            // If it should start collapsed
            if (this.options.collapsed) {
                $trigger(this.toggle_button, 'click');
            }

            // Collapse button disabled
            if (this.schema.options && typeof this.schema.options.disable_collapse !== "undefined") {
                if (this.schema.options.disable_collapse) this.toggle_button.style.display = 'none';
            }
            else if (this.jsoneditor.options.disable_collapse) {
                this.toggle_button.style.display = 'none';
            }

            // Edit JSON Button
            this.editjson_button = this.getButton('JSON', 'edit', 'Edit JSON');
            this.editjson_button.classList.add('json-editor-btntype-editjson');
            this.editjson_button.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                self.toggleEditJSON();
            });
            this.editjson_controls.appendChild(this.editjson_button);
            this.editjson_controls.appendChild(this.editjson_holder);

            // Edit JSON Buttton disabled
            if (this.schema.options && typeof this.schema.options.disable_edit_json !== "undefined") {
                if (this.schema.options.disable_edit_json) this.editjson_button.style.display = 'none';
            }
            else if (this.jsoneditor.options.disable_edit_json) {
                this.editjson_button.style.display = 'none';
            }

            // Object Properties Button
            this.addproperty_button = this.getButton('Properties', 'edit', self.translate('button_object_properties'));
            this.addproperty_button.classList.add('json-editor-btntype-properties');
            this.addproperty_button.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                self.toggleAddProperty();
            });
            this.addproperty_controls.appendChild(this.addproperty_button);
            this.addproperty_controls.appendChild(this.addproperty_holder);
            this.refreshAddProperties();

            // non required properties start deactivated
            this.deactivateNonRequiredProperties();

        }

        // Fix table cell ordering
        if (this.options.table_row) {
            this.editor_holder = this.container;
            $each(this.property_order, function (i, key) {
                self.editor_holder.appendChild(self.editors[key].container);
            });
        }
        // Layout object editors in grid if needed
        else {
            // Initial layout
            this.layoutEditors();
            // Do it again now that we know the approximate heights of elements
            this.layoutEditors();
        }
    },
    //saveJSON: function () {
    //    if (!this.editjson_holder) return;

    //    try {
    //        var json = JSON.parse(this.editjson_textarea.value);
    //        this.setValue(json);
    //        this.hideEditJSON();
    //        this.onChange(true);
    //    }
    //    catch (e) {
    //        window.alert('invalid JSON');
    //        throw e;
    //    }
    //},
    showEditJSON: function () {
        //if (!this.editjson_holder) return;
        //this.hideAddProperty();

        // Position the form directly beneath the button
        // TODO: edge detection

        var input = this.jsoneditor.input;
        var holder = input.editors.d.header.parentNode;
        var buttonData = holder.querySelector("#data");

        this.editjson_holder.style.left = buttonData.offsetLeft + 11 + "px";
        this.editjson_holder.style.top = buttonData.offsetTop + buttonData.offsetHeight - 17 + "px";

        // Start the textarea with the current value
        this.editjson_textarea.value = getData(input).d;

        // Disable the rest of the form while editing JSON
        this.disable();

        this.editjson_holder.style.display = '';
        buttonData.disabled = false;
        this.editing_json = true;
    }
});

JSONEditor.defaults.editors.array = JSONEditor.defaults.editors.array.extend({
    getHeader: function (text, root) {
        var el = null;
        if (root) {
            el = document.createElement('h4');
            el.setAttribute('tag', 'h4');
        }
        else {
            var el = document.createElement('div');
        }

        el.style.paddingTop = el.style.paddingBottom = el.style.marginTop = el.style.marginBottom = '0px';
        if (typeof text === "string") {
            el.textContent = text;
        }
        else {
            el.appendChild(text);
        }
        return el;
    },
    getDescription: function (text, root) {
        var el = document.createElement('p');
        el.style.paddingTop = el.style.paddingBottom = el.style.marginTop = el.style.marginBottom = '0px';
        el.style.marginBottom = '2px';
        el.setAttribute('tag', 'description');
        if (window.DOMPurify) el.innerHTML = window.DOMPurify.sanitize(text);
        else el.textContent = this.cleanText(text);

        if (root) {
            el.style.marginTop = '8px';
        }
        else {
            //el.style.marginTop = '5px';
            el.classList.add('help-block');
        }
        return el;
    },
    build: function () {
        var self = this;

        if (!this.options.compact) {
            this.header = document.createElement('label');
            this.header.textContent = this.getTitle();
            this.title = this.getHeader(this.header, "root.d" == this.path);
            this.container.appendChild(this.title);
            this.title_controls = this.theme.getHeaderButtonHolder();
            this.title.appendChild(this.title_controls);
            if (this.schema.description) {
                this.description = this.getDescription(this.schema.description, "root.d" == this.path);
                this.container.appendChild(this.description);
            }
            this.error_holder = document.createElement('div');
            this.container.appendChild(this.error_holder);

            if (this.schema.format === 'tabs-top') {
                this.controls = this.theme.getHeaderButtonHolder();
                this.title.appendChild(this.controls);
                this.tabs_holder = this.theme.getTopTabHolder(this.getValidId(this.getItemTitle()));
                this.container.appendChild(this.tabs_holder);
                this.row_holder = this.theme.getTopTabContentHolder(this.tabs_holder);

                this.active_tab = null;
            }
            else if (this.schema.format === 'tabs') {
                this.controls = this.theme.getHeaderButtonHolder();
                this.title.appendChild(this.controls);
                this.tabs_holder = this.theme.getTabHolder(this.getValidId(this.getItemTitle()));
                this.container.appendChild(this.tabs_holder);
                this.row_holder = this.theme.getTabContentHolder(this.tabs_holder);

                this.active_tab = null;
            }
            else {
                this.panel = this.theme.getIndentedPanel();
                this.container.appendChild(this.panel);
                this.row_holder = document.createElement('div');
                this.panel.appendChild(this.row_holder);
                this.controls = this.theme.getButtonHolder();
                if (this.array_controls_top) {
                    this.title.appendChild(this.controls);
                }
                else {
                    this.panel.appendChild(this.controls);
                }
            }
        }
        else {
            this.panel = this.theme.getIndentedPanel();
            this.container.appendChild(this.panel);
            this.title_controls = this.theme.getHeaderButtonHolder();
            this.panel.appendChild(this.title_controls);
            this.controls = this.theme.getButtonHolder();
            this.panel.appendChild(this.controls);
            this.row_holder = document.createElement('div');
            this.panel.appendChild(this.row_holder);
        }

        // Edit JSON modal
        this.editjson_holder = this.theme.getModal();
        this.editjson_textarea = this.theme.getTextareaInput();
        this.editjson_textarea.style.marginTop = "0px";
        this.editjson_textarea.style.height = '170px';
        this.editjson_textarea.style.width = '300px';
        this.editjson_textarea.style.display = 'block';
        this.editjson_save = this.getButton('Save', 'save', 'Save');
        this.editjson_save.classList.add('json-editor-btntype-save');
        this.editjson_save.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();
            self.saveJSON();
        });
        this.editjson_copy = this.getButton('Copy', 'copy', 'Copy');
        this.editjson_copy.classList.add('json-editor-btntype-copy');
        this.editjson_copy.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();
            self.copyJSON();
        });
        this.editjson_cancel = this.getButton('Cancel', 'cancel', 'Cancel');
        this.editjson_cancel.classList.add('json-editor-btntype-cancel');
        this.editjson_cancel.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();
            self.hideEditJSON();
        });
        this.editjson_holder.appendChild(this.editjson_textarea);
        this.editjson_holder.appendChild(this.editjson_save);
        this.editjson_holder.appendChild(this.editjson_copy);
        this.editjson_holder.appendChild(this.editjson_cancel);

        this.editjson_controls = this.theme.getHeaderButtonHolder();
        this.title.appendChild(this.editjson_controls);

        // Edit JSON Button
        //this.editjson_button = this.getButton('JSON', 'edit', 'Edit JSON');
        //this.editjson_button.classList.add('json-editor-btntype-editjson');
        //this.editjson_button.addEventListener('click', function (e) {
        //    e.preventDefault();
        //    e.stopPropagation();
        //    self.toggleEditJSON();
        //});
        //this.editjson_controls.appendChild(this.editjson_button);
        this.editjson_controls.appendChild(this.editjson_holder);

        // Add controls
        this.addControls();
    },
    toggleEditJSON: function () {
        if (this.editing_json) this.hideEditJSON();
        else this.showEditJSON();
    },
    hideEditJSON: function () {
        if (!this.editjson_holder) return;
        if (!this.editing_json) return;

        this.editjson_holder.style.display = 'none';
        this.enable();
        this.editing_json = false;
    },
    showEditJSON: function () {
        //if (!this.editjson_holder) return;
        //this.hideAddProperty();

        // Position the form directly beneath the button
        // TODO: edge detection

        var input = this.jsoneditor.input;
        var holder = input.editors.d.header.parentNode;
        var buttonData = holder.querySelector("#data");

        this.editjson_holder.style.left = buttonData.offsetLeft + 11 + "px";
        this.editjson_holder.style.top = buttonData.offsetTop + buttonData.offsetHeight - 17 + "px";

        // Start the textarea with the current value
        this.editjson_textarea.value = getData(input).d;

        // Disable the rest of the form while editing JSON
        this.disable();

        this.editjson_holder.style.display = '';
        buttonData.disabled = false;
        this.editing_json = true;
    },
    addControls: function () {
        var self = this;
        this.panel = self.container.querySelector("div[tag='panel']");
        this.panel.removeAttribute('class');

        //this.collapsed = false;
        //this.toggle_button = this.getButton('', 'collapse', this.translate('button_collapse'));
        //this.toggle_button.setAttribute('tag', 'toggle');
        //this.toggle_button.setAttribute('collapsed', this.collapsed);
        //this.toggle_button.classList.add('json-editor-btntype-toggle');
        //this.title_controls.appendChild(this.toggle_button);
        //var row_holder_display = self.row_holder.style.display;
        //var controls_display = self.controls.style.display;
        //this.toggle_button.addEventListener('click', function (e) {
        //    e.preventDefault();
        //    e.stopPropagation();
        //    if (self.collapsed) {
        //        self.collapsed = false;
        //        if (self.panel) self.panel.style.display = '';
        //        self.row_holder.style.display = row_holder_display;
        //        if (self.tabs_holder) self.tabs_holder.style.display = '';
        //        self.controls.style.display = controls_display;
        //        self.setButtonText(this, '', 'collapse', self.translate('button_collapse'));
        //    }
        //    else {
        //        self.collapsed = true;
        //        self.row_holder.style.display = 'none';
        //        if (self.tabs_holder) self.tabs_holder.style.display = 'none';
        //        self.controls.style.display = 'none';
        //        if (self.panel) self.panel.style.display = 'none';
        //        self.setButtonText(this, '', 'expand', self.translate('button_expand'));
        //    }
        //    self.toggle_button.setAttribute('collapsed', self.collapsed);
        //});

        //// If it should start collapsed
        //if (this.options.collapsed) {
        //    $trigger(this.toggle_button, 'click');
        //}

        //// Collapse button disabled
        //if (this.schema.options && typeof this.schema.options.disable_collapse !== "undefined") {
        //    if (this.schema.options.disable_collapse) this.toggle_button.style.display = 'none';
        //}
        //else if (this.jsoneditor.options.disable_collapse) {
        //    this.toggle_button.style.display = 'none';
        //}

        // Add "new row" and "delete last" buttons below editor

        function click() {
            var i = self.rows.length;
            if (self.row_cache[i]) {
                self.rows[i] = self.row_cache[i];
                self.rows[i].setValue(self.rows[i].getDefault(), true);
                self.rows[i].container.style.display = '';
                if (self.rows[i].tab) self.rows[i].tab.style.display = '';
                self.rows[i].register();
            }
            else {
                self.addRow();
            }
            self.active_tab = self.rows[i].tab;
            self.refreshTabs();
            self.refreshValue();
            self.onChange(true);
            self.jsoneditor.trigger('addRow');

            setEdit(self.container, !self.jsoneditor.root.schema.edit);

            self.panel.style.display = '';

            var row = self.rows[i].container;
            if ("object" == row.getAttribute("data-schematype")) {
                if (1 < self.rows.length) {
                    row.style.paddingTop = "5px";
                }
            }
            else {
                row.style.paddingBottom = "5px";
            }
        }

        if (fileform === this.formname) {
            this.add_row_button = document.createElement('a');
            this.add_row_button.type = 'a';
            this.add_row_button.style.display = '';
            this.add_row_button.style.borderTopRightRadius = '4px';
            this.add_row_button.style.borderBottomRightRadius = '4px';
            this.setButtonText(this.add_row_button, this.getItemTitle(), 'add', this.translate('button_add_row_title', [this.getItemTitle()]));

            var file = document.createElement('input');
            file.type = 'file';
            file.id = "file";
            this.add_row_button.classList.add("btn", "json-editor-btntype-add", "a-upload");
            this.add_row_button.appendChild(file);
            file.addEventListener('change', function (e) {
                e.preventDefault();
                e.stopPropagation();
                if (0 < e.target.files.length) {
                    click();
                }
            }, false);
        }
        else {
            this.add_row_button = this.getButton(this.getItemTitle(), 'add', this.translate('button_add_row_title', [this.getItemTitle()]));
            this.add_row_button.classList.add('json-editor-btntype-add');
            this.add_row_button.style.borderTopRightRadius = '4px';
            this.add_row_button.style.borderBottomRightRadius = '4px';
            this.add_row_button.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                click();
            });
        }

        self.controls.appendChild(this.add_row_button);

        this.delete_last_row_button = this.getButton(this.translate('button_delete_last', [this.getItemTitle()]), 'delete', this.translate('button_delete_last_title', [this.getItemTitle()]));
        this.delete_last_row_button.classList.add('json-editor-btntype-deletelast');
        this.delete_last_row_button.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();

            if (!self.askConfirmation()) {
                return false;
            }

            var rows = self.getValue();
            var new_active_tab = null;

            rows.pop();
            self.empty(true);
            self.setValue(rows);

            if (self.rows[self.rows.length - 1]) {
                new_active_tab = self.rows[self.rows.length - 1].tab;
            }

            if (new_active_tab) {
                self.active_tab = new_active_tab;
                self.refreshTabs();
            }

            self.onChange(true);
            self.jsoneditor.trigger('deleteRow');
        });
        self.controls.appendChild(this.delete_last_row_button);

        this.remove_all_rows_button = this.getButton(this.translate('button_delete_all'), 'delete', this.translate('button_delete_all_title'));
        this.remove_all_rows_button.classList.add('json-editor-btntype-deleteall');
        this.remove_all_rows_button.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();

            if (!self.askConfirmation()) {
                return false;
            }

            self.empty(true);
            self.setValue([]);
            self.onChange(true);
            self.jsoneditor.trigger('deleteAllRows');

            if (0 == self.rows.length) {
                self.panel.style.display = 'none';
            }
        });
        self.controls.appendChild(this.remove_all_rows_button);

        if (self.tabs) {
            this.add_row_button.style.width = '100%';
            this.add_row_button.style.textAlign = 'left';
            this.add_row_button.style.marginBottom = '3px';

            this.delete_last_row_button.style.width = '100%';
            this.delete_last_row_button.style.textAlign = 'left';
            this.delete_last_row_button.style.marginBottom = '3px';

            this.remove_all_rows_button.style.width = '100%';
            this.remove_all_rows_button.style.textAlign = 'left';
            this.remove_all_rows_button.style.marginBottom = '3px';
        }

        if (fileform !== this.formname) {
            this.add_row_button.setAttribute('tag', 'input');
            this.panel.setAttribute('tag', 'input');
            this.remove_all_rows_button.setAttribute('tag', 'array');
        }

        this.panel.style.display = 'none';
    },

    //copyJSON: function () {
    //    if (!this.editjson_holder) return;
    //    var ta = document.createElement('textarea');
    //    ta.value = this.editjson_textarea.value;
    //    ta.setAttribute('readonly', '');
    //    ta.style.position = 'absolute';
    //    ta.style.left = '-9999px';
    //    document.body.appendChild(ta);
    //    ta.select();
    //    document.execCommand('copy');
    //    document.body.removeChild(ta);
    //},
    //saveJSON: function () {
    //    if (!this.editjson_holder) return;

    //    try {
    //        var json = JSON.parse(this.editjson_textarea.value);
    //        this.setValue(json);
    //        this.hideEditJSON();
    //        this.onChange(true);
    //    }
    //    catch (e) {
    //        window.alert('invalid JSON');
    //        throw e;
    //    }
    //},

    //hideAddProperty: function () {
    //    if (!this.addproperty_holder) return;
    //    if (!this.adding_property) return;

    //    this.addproperty_holder.style.display = 'none';
    //    this.enable();

    //    this.adding_property = false;
    //},
    addRow: function (value, initial) {
        var self = this;
        var i = this.rows.length;

        self.rows[i] = this.getElementEditor(i);
        self.row_cache[i] = self.rows[i];

        if (fileform === this.formname) {
            self.rows[i].input.removeAttribute("tag");
            self.rows[i].input.style.display = 'none';

            var file = self.header.parentNode.querySelector("#file");
            if ((null !== file && undefined !== file) && 0 < file.files.length) {
                self.rows[i].label.setAttribute("tag", "file");
                self.rows[i].label.file = file.files[0];
                self.rows[i].label.innerText = file.files[0].name + "  " + renderSize(file.files[0].size);
                file.value = '';
            }
        }

        if (self.tabs_holder) {
            self.rows[i].tab_text = document.createElement('span');
            self.rows[i].tab_text.textContent = self.rows[i].getHeaderText();
            if (self.schema.format === 'tabs-top') {
                self.rows[i].tab = self.theme.getTopTab(self.rows[i].tab_text, this.getValidId(self.rows[i].path));
                self.theme.addTopTab(self.tabs_holder, self.rows[i].tab);
            }
            else {
                self.rows[i].tab = self.theme.getTab(self.rows[i].tab_text, this.getValidId(self.rows[i].path));
                self.theme.addTab(self.tabs_holder, self.rows[i].tab);
            }
            self.rows[i].tab.addEventListener('click', function (e) {
                self.active_tab = self.rows[i].tab;
                self.refreshTabs();
                e.preventDefault();
                e.stopPropagation();
            });
        }

        var controls_holder = self.rows[i].title_controls || self.rows[i].array_controls;

        // Buttons to delete row, move row up, and move row down
        if (!self.hide_delete_buttons) {
            self.rows[i].delete_button = this.getButton(self.getItemTitle(), 'delete', this.translate('button_delete_row_title', [self.getItemTitle()]));
            self.rows[i].delete_button.classList.add('delete', 'json-editor-btntype-delete');
            self.rows[i].delete_button.setAttribute('data-i', i);
            self.rows[i].delete_button.setAttribute('tag', 'del');
            self.rows[i].delete_button.style.borderTopRightRadius = '4px';
            self.rows[i].delete_button.style.borderBottomRightRadius = '4px';
            self.rows[i].delete_button.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();

                if (!self.askConfirmation()) {
                    return false;
                }

                var i = this.getAttribute('data-i') * 1;
                var value = self.getValue();
                var newval = [];
                var new_active_tab = null;

                $each(value, function (j, row) {
                    if (j !== i) {
                        newval.push(row);
                    }
                });

                if (null !== file && undefined !== file) {
                    var files = [];
                    $each(self.rows, function (j, row) {
                        if (j !== i) {
                            files.push(row.label.file);
                        }
                    });
                }

                self.empty(true);
                self.setValue(newval);

                if (null !== file && undefined !== file) {
                    $each(files, function (j, row) {
                        self.rows[j].label.setAttribute("tag", "file");
                        self.rows[j].label.file = row;
                        self.rows[j].label.innerText = row.name + "  " + renderSize(row.size);
                    });
                }

                if (self.rows[i]) {
                    new_active_tab = self.rows[i].tab;
                } else if (self.rows[i - 1]) {
                    new_active_tab = self.rows[i - 1].tab;
                }

                if (new_active_tab) {
                    self.active_tab = new_active_tab;
                    self.refreshTabs();
                }

                self.onChange(true);
                self.jsoneditor.trigger('deleteRow');

                if (0 == self.rows.length) {
                    self.panel.style.display = 'none';
                }

                self.rows.forEach(c => {
                    var row = c.container;
                    if ("object" == row.getAttribute("data-schematype")) {
                        if (1 < self.rows.length) {
                            row.style.paddingTop = "5px";
                        }
                    }
                    else {
                        row.style.paddingBottom = "5px";
                    }
                });
            });

            if (controls_holder) {
                controls_holder.appendChild(self.rows[i].delete_button);
            }
        }

        if (null == file || undefined == file) {
            //Button to copy an array element and add it as last element
            if (self.show_copy_button) {
                self.rows[i].copy_button = this.getButton(self.getItemTitle(), 'copy', 'Copy ' + self.getItemTitle());
                self.rows[i].copy_button.classList.add('copy', 'json-editor-btntype-copy');
                self.rows[i].copy_button.setAttribute('data-i', i);
                self.rows[i].copy_button.addEventListener('click', function (e) {
                    var value = self.getValue();
                    e.preventDefault();
                    e.stopPropagation();
                    var i = this.getAttribute('data-i') * 1;

                    $each(value, function (j, row) {
                        if (j === i) {
                            value.push(row);
                        }
                    });

                    self.setValue(value);
                    self.refreshValue(true);
                    self.onChange(true);
                });

                controls_holder.appendChild(self.rows[i].copy_button);
            }

            if (i && !self.hide_move_buttons) {
                self.rows[i].moveup_button = this.getButton('', 'moveup', this.translate('button_move_up_title'));
                self.rows[i].moveup_button.classList.add('moveup', 'json-editor-btntype-move');
                self.rows[i].moveup_button.setAttribute('data-i', i);
                self.rows[i].moveup_button.addEventListener('click', function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                    var i = this.getAttribute('data-i') * 1;

                    if (i <= 0) return;
                    var rows = self.getValue();
                    var tmp = rows[i - 1];
                    rows[i - 1] = rows[i];
                    rows[i] = tmp;

                    self.setValue(rows);
                    self.active_tab = self.rows[i - 1].tab;
                    self.refreshTabs();

                    self.onChange(true);

                    self.jsoneditor.trigger('moveRow');
                });

                if (controls_holder) {
                    controls_holder.appendChild(self.rows[i].moveup_button);
                }
            }

            if (!self.hide_move_buttons) {
                self.rows[i].movedown_button = this.getButton('', 'movedown', this.translate('button_move_down_title'));
                self.rows[i].movedown_button.classList.add('movedown', 'json-editor-btntype-move');
                self.rows[i].movedown_button.setAttribute('data-i', i);
                self.rows[i].movedown_button.addEventListener('click', function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                    var i = this.getAttribute('data-i') * 1;

                    var rows = self.getValue();
                    if (i >= rows.length - 1) return;
                    var tmp = rows[i + 1];
                    rows[i + 1] = rows[i];
                    rows[i] = tmp;

                    self.setValue(rows);
                    self.active_tab = self.rows[i + 1].tab;
                    self.refreshTabs();
                    self.onChange(true);

                    self.jsoneditor.trigger('moveRow');
                });

                if (controls_holder) {
                    controls_holder.appendChild(self.rows[i].movedown_button);
                }
            }
        }

        if (value) self.rows[i].setValue(value, initial);
        self.refreshTabs();
    },
    getElementEditor: function (i) {
        var item_info = this.getItemInfo(i);
        var schema = this.getItemSchema(i);
        schema = this.jsoneditor.expandRefs(schema);
        schema.title = item_info.title + ' ' + (i + 1);

        var editor = this.jsoneditor.getEditorClass(schema);

        var holder;
        if (this.tabs_holder) {
            if (this.schema.format === 'tabs-top') {
                holder = this.theme.getTopTabContent();
            }
            else {
                holder = this.theme.getTabContent();
            }
            holder.id = this.path + '.' + i;
        }
        else if (item_info.child_editors) {
            holder = this.theme.getChildEditorHolder();
        }
        else {
            holder = this.theme.getIndentedPanel();
        }

        this.row_holder.appendChild(holder);

        var ret = this.jsoneditor.createEditor(editor, {
            jsoneditor: this.jsoneditor,
            schema: schema,
            container: holder,
            path: this.path + '.' + i,
            parent: this,
            required: true
        });
        ret.preBuild();
        ret.build();
        ret.postBuild();

        if (!ret.title_controls) {
            ret.array_controls = this.theme.getButtonHolder();
            holder.appendChild(ret.array_controls);
        }

        ret.header.style.marginTop = "2px";
        ret.header.style.marginBottom = "0px";

        var h4 = ret.header.parentNode;
        if ("H4" == h4.tagName) {
            var el = document.createElement('div');
            el.style.paddingTop = el.style.paddingBottom = el.style.marginTop = el.style.marginBottom = '0px';
            var next = null
            for (var i = h4.children.length - 1; i >= 0; i--) {
                next = el.insertBefore(h4.children[i], next);
            }

            var parent = h4.parentNode;
            next = h4.nextSibling;
            parent.removeChild(h4);
            parent.insertBefore(el, next);
        }

        return ret;
    }
});

var Iconlibs = JSONEditor.defaults.iconlibs.bootstrap3;
JSONEditor.defaults.iconlibs.bootstrap3 = function () {
    var icon = new Iconlibs();
    icon.mapping = {
        collapse: 'chevron-down',
        expand: 'chevron-right',
        "delete": 'remove',
        edit: 'edit',
        add: 'plus',
        cancel: 'floppy-remove',
        save: 'floppy-saved',
        moveup: 'arrow-up',
        movedown: 'arrow-down',
        clear: 'remove-circle',
        time: 'time',
        calendar: 'calendar',
        data: 'list-alt'
    };

    return icon;
};

var disclosure = null;
var doc = null;
//var businessKey = null;
var businessName = null;
//var business = null;
var compiled_tpl = juicer(document.getElementById('tpl').innerHTML);
var compiled_menuGroup = juicer(document.getElementById('menuGroup').innerHTML);
var compiled_benchmark = juicer(document.getElementById('benchmark').innerHTML);
var menus = document.getElementById('menus');
var edit = false;
var businessSelect = document.getElementById('business');
var groupSelect = document.querySelector("#group");
var members = document.getElementById('members');
var businessDescription = document.getElementById('business_description');
var tokenvalue = null;
var main = document.getElementById('main');
var close_sidebar = document.getElementById('close-sidebar');
var menu_toggle = document.getElementById('menu-toggle');
var wrapper = document.getElementById('wrapper');
var page_content = document.getElementById('page-content');
menu_toggle.addEventListener('click', function (e) {
    e.preventDefault();
    e.stopPropagation();
    wrapper.classList.toggle("toggled");
    e.currentTarget.classList.toggle("is-active");
}, false);
document.getElementById('search').addEventListener('click', function (e) {
    e.preventDefault();
    e.stopPropagation();
    var value = document.getElementById('searchValue').value.trim();
    if ("" !== value) {
        var target = null;
        var aliass = members.querySelectorAll("#alias");
        for (var i = 0; i < aliass.length; i++) {
            if (-1 < aliass[i].textContent.indexOf(value)) {
                target = aliass[i].getAttribute("tag");
                document.getElementById(target).scrollIntoView({ behavior: 'smooth' });
                break;
            }
        }
    }
}, false);

function businessOnchang(obj) {
    if (-1 !== obj.selectedIndex) {
        var select = obj.options[obj.selectedIndex];
        //businessKey = select.value;
        businessName = select.value;//select.text;
        var business = doc[businessName];

        var navigationName = document.querySelector(".sidebar-item > .user-info > .user-name strong");
        navigationName.textContent = business.alias;

        if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)) {
            main.style.paddingLeft = main.style.paddingRight = 0;
        }

        if (business.config.navigtion) {
            close_sidebar.style.removeProperty("display");

            if (!wrapper.classList.contains("toggled")) {
                wrapper.classList.toggle("toggled");
            }
            if (!menu_toggle.classList.contains("is-active")) {
                menu_toggle.classList.toggle("is-active");
            }

            var html = compiled_menuGroup.render(business.docGroup);
            menus.innerHTML = html;

            document.querySelectorAll(".sidebar-submenu a").forEach(c => {
                c.addEventListener('click', e => {
                    e.preventDefault();
                    e.stopPropagation();
                    var target = members.querySelector("div[tag='" + e.currentTarget.id + "']");
                    if (null != target) {
                        target.scrollIntoView({ behavior: 'smooth' });
                    }
                }, false);
            });

            // Dropdown menu
            $(".sidebar-dropdown > a").click(function () {
                //$(".sidebar-submenu").slideUp(200);
                if ($(this).parent().hasClass("active")) {
                    //$(".sidebar-dropdown").removeClass("active");
                    $(this).next(".sidebar-submenu").slideUp(200);
                    $(this).parent().removeClass("active");
                } else {
                    //$(".sidebar-dropdown").removeClass("active");
                    $(this).next(".sidebar-submenu").slideDown(200);
                    $(this).parent().addClass("active");
                }
            });
        }
        else {
            if (wrapper.classList.contains("toggled")) {
                wrapper.classList.toggle("toggled");
            }
            if (menu_toggle.classList.contains("is-active")) {
                menu_toggle.classList.toggle("is-active");
            }

            close_sidebar.style.display = "none";
        }

        businessDescription.innerHTML = '';
        if (business.description) {
            businessDescription.innerHTML = business.description;
            businessDescription.parentNode.style.display = "";
        }
        else {
            businessDescription.parentNode.style.display = "none";
        }

        groupSelect.style.display = business.config.groupEnable ? "" : "none";

        groupSelect.options.length = 0;
        var index = -1;
        var count = 0;
        for (var i in business.group) {
            if (i === business.config.groupSelect) {
                index = count;
            }
            else if (-1 === index && i === business.groupDefault) {
                index = count;
            }

            count++;
            groupSelect.options.add(new Option(i, i));
        }

        members.innerHTML = '';

        if (0 == count) { return; }

        groupSelect.options.selectedIndex = -1 === index ? 0 : index;
        groupOnchang(groupSelect);
    }
}

function groupOnchang(obj) {
    if (null != doc && null != businessName) {
        //console.time("timer");
        load(doc[businessName].group[obj.options[obj.selectedIndex].value]);
        //console.timeEnd("timer");
    }
}

function destroy(all = false) {
    if (null != disclosure) {
        disclosure.destroy();
        disclosure = null;
    }
    members.innerHTML = '';
    tokenvalue = null;

    if (all) {
        businessDescription.innerHTML = '';
        groupSelect.options.length = 0;
        doc = null;
        businessName = null;
    }
}

function go() {
    destroy(true);

    var url = document.getElementById('url').value;
    if (url == null || url == undefined || url === '') {
        url = document.location.origin + "/business.doc"
    }
    else {
        var http = "http://*:";
        var https = "https://*:";
        var http2 = "http://+:";
        var https2 = "https://+:";
        if (0 == url.toLowerCase().indexOf(http) || 0 == url.toLowerCase().indexOf(https) || 0 == url.toLowerCase().indexOf(http2) || 0 == url.toLowerCase().indexOf(https2)) {
            url = url.replace('*', document.location.hostname).replace('+', document.location.hostname);
        }
    }
    document.getElementById('url').setAttribute("value", url);

    //atomic(document.querySelector("#url").value)
    //    .then(function (response) {
    //        //console.log(response.data); // xhr.responseText
    //        //console.log(response.xhr);  // full response
    //        doc = response.data;
    //        var def = count = 0;
    //        var groupElement = document.querySelector("#group");
    //        for (var i in doc.group) {
    //            if (i === doc.groupDefault) {
    //                def = count;
    //            }
    //            count++;
    //            groupElement.options.add(new Option(i, i));
    //        }

    //        if (0 == count) { return; }

    //        groupElement.options.selectedIndex = def;
    //        groupOnchang(document.querySelector("#group"));
    //    })
    //    .catch(function (error) {
    //        console.log(error);
    //    });
    ajax.get(url, null,
        function (response) {
            try {
                doc = JSON.parse(response);
                //var businessSelect = document.getElementById('business');
                for (var i in doc) {
                    businessSelect.options.add(new Option(doc[i].alias, i));

                    if (doc[i].config.host == null || doc[i].config.host == undefined || doc[i].config.host === '') {
                        doc[i].config.host = document.location.origin;
                    }
                }
                businessOnchang(businessSelect);
            } catch (e) {
                console.log(e);
            }
        }, function (response) {
            console.log(response);
        });
}

function expand(ev) {
    var e = ev || event;
    if ((0 && 1) !== e.button) {
        return;
    }
    var parent = e.srcElement.parentNode.parentNode.parentNode.parentNode;
    var id = parent.id + "_member";
    var member = parent.querySelector("#" + id);

    if (null !== member && member.member) {
        var inputid = parent.id + '_input_out';
        var input = parent.querySelector("#" + inputid);

        if ('' === input.innerHTML) {
            loadMember(member.member);

            setEdit(input, edit);
            setInput(input, member.member.hasReturn);
            setSdk(parent.querySelector("div[id='SDK & Debug']"));
        }
    }
}

function load(m) {

    destroy();

    var members2 = []
    for (var i in m) {
        members2.push(m[i]);
    }

    var html = compiled_tpl.render(members2);
    members.innerHTML = html;
    //console.time("timer");
    //=============================================//
    members2.forEach(member => {
        var id = member.name + "_member";
        var member2 = members.querySelector("#" + id);
        member2.member = member;
    });

    disclosure = new Houdini('[data-houdini]', { btnClass: 'btn btn-primary' });

    //console.timeEnd("timer");
}

function loadMember(member) {

    //==================input==================//
    var inputid = member.name + '_input_out';

    var input = {
        title: "Input",
        type: "object",
        edit: !edit,
        argSingle: member.argSingle,
        httpFile: member.httpFile,
        testing: member.testing,
        name: member.name,
        properties: {
            t: {},
            d: {
                id: member.name + '.d',
                title: "d (Array)",
                type: "object",
                description: "API data",
                properties: {}
            },
            f: {
                title: "files",
                type: "array",
                options: {
                    disable_array_delete_last_row: true,
                    array_controls_top: true
                },
                items: {
                    type: "string",
                    title: "file"
                }
            }
        }
    };

    var out = {
        title: "Out",
        type: "object",
        properties: {}
    }

    var hasToken = false;
    var token = null;
    for (var i in member.properties) {
        if (member.properties[i].token) {
            hasToken = true;
            token = member.properties[i];
        } else {
            if (member.argSingle) {
                input.properties.d = member.properties[i];
                input.properties.d.id = member.name + '.d';

                var title = member.properties[i].lastType;
                if (member.properties[i].array) {
                    title += " Array";
                }
                input.properties.d.title = "d (" + title + ")";
            }
            else {
                input.properties.d.properties[i] = member.properties[i];
            }
        }
    }

    if (null != member.returns) {
        if ("object" == member.returns.type) {
            out.properties = member.returns.properties;
            out.description = member.returns.description;
        }
        else {
            out.properties[member.returns.id] = member.returns;
        }
    }

    if (!hasToken) {
        delete input.properties.t;
    }
    else {
        input.properties.t = token;
        input.properties.t.id = member.name + '.t';
        input.properties.t.title = "t (String)";
        if (null != tokenvalue && '' !== tokenvalue) {
            input.properties.t.default = tokenvalue;
        }
    }

    if ("{}" == JSON.stringify(input.properties.d.properties)) {
        delete input.properties.d;
    }

    if (!member.httpFile) {
        delete input.properties.f;
    }

    //if (input.properties.t || input.properties.d) {
    //    input.type = "null";
    //}

    var inputouteditor = new JSONEditor(document.getElementById(inputid), {
        // The schema for the editor
        schema: {
            title: "Input & Out",
            type: "object",
            format: "categories",
            properties: {
                input: input,
                out: out
            }
        }
    });

    inputouteditor.input = inputouteditor.editors["root.input"];
    inputouteditor.out = inputouteditor.editors["root.out"];
    ready(inputouteditor);

    //==================sdk==================//
    var sdkid = member.name + '_sdk';
    inputouteditor.sdkeditor = new JSONEditor(document.getElementById(sdkid), {
        // Enable fetching schemas via ajax
        //ajax: true,
        schema: {
            title: "SDK & Debug",
            type: "object",
            format: "categories",
            properties: {
                Curl: getSdk("sh"),
                JavaScript: getSdk("javascript"),
                NET: getSdk("csharp"),
                Java: getSdk("java"),
                PHP: getSdk("php"),
                Debug: getSdk("json")
            }
        }
    });

    //document.querySelector('#' + sdkid + ' > div > div.well.well-sm > div > div').style.cssText = 'padding: 0px;border: 0px;margin: 0px;';
    //========================================//
}

function setInput(input, hasReturn) {
    if (null == input) { return; }

    input.querySelectorAll("p[tag='description']").forEach(c => {
        var row = c.parentNode.parentNode.parentNode.nextSibling;

        if ("array" == c.parentNode.getAttribute("data-schematype")) {
            row = c.parentNode.parentNode.nextSibling;
        }

        var h5 = c.querySelector("h5");
        if (null != h5 && null != row && row.className === "row") {
            c.style.marginBottom = '8px';
        }
    });

    input.querySelectorAll("div[tag='row']").forEach(c => {
        if (null == c.nextSibling) {
            if (null != c.querySelector("h5")) {
                c.style.marginBottom = '8px';
            }

            var p = c.querySelector("p[tag='description']");
            if (null != p) {
                p.style.marginBottom = '0px';
            }
        }
    });

    input.querySelectorAll("div[tag='panel']").forEach(c => {
        var rows = c.querySelectorAll("div[tag='row']");
        rows.forEach(row => {
            var array = row.querySelector("[data-schematype='array']");
            if (null !== array) {
                for (i in array.children) {
                    var el = array.children[i];
                    if ("P" == el.tagName) {
                        var h5 = el.querySelector("h5:last-child");
                        if (null != h5) {
                            h5.style.marginBottom = '2px';
                        }
                        break;
                    }
                }
            }

            var object = row.querySelector("[data-schematype='object']");
            if (null !== object) {
                for (i in object.children) {
                    var el = object.children[i];
                    if ("P" == el.tagName) {
                        var h5 = el.querySelector("h5:last-child");
                        if (null != h5) {
                            h5.style.marginBottom = '15px';
                        }
                        break;
                    }
                }
            }
        });
    });

    var input_out = input.querySelector("div[id='Input & Out']");
    input_out.style.border = input_out.style.margin = input_out.style.paddingTop = input_out.style.paddingBottom = "0px";
    input_out.className = "tab-content";

    input.querySelector('#Input > div > div > h4').style.display = 'none';
    input.querySelector('#Out > div > div > h4').style.display = 'none';

    var panel = input.querySelector("#Input > div[tag='row']");
    panel.removeAttribute("style");
    var panel2 = input.querySelector("#Input > div > div > div[tag='panel']");
    panel2.removeAttribute("class"); panel2.removeAttribute("style"); panel2.style.margin = "8px";

    var out = input.querySelector("#Out > div[tag='row']");
    out.removeAttribute("style");
    var out2 = input.querySelector("#Out > div > div > div[tag='panel']");
    out2.removeAttribute("class"); out2.removeAttribute("style"); out2.style.margin = "8px";

    if (!hasReturn) {
        input.querySelector('#Out > div').style.display = 'none';
    }
}

function setEdit(root, edit) {
    if (!edit) {
        root.querySelectorAll("[tag='input']").forEach(c => { c.style.display = "none"; });
        root.querySelectorAll("[tag='description']").forEach(c => { c.style.display = ""; });

        root.querySelectorAll("button[tag='array']").forEach(c => {
            if ("" == c.style.display) {
                c.style.display = "none";
            }
        });
    }
    else {
        root.querySelectorAll("[tag='input']").forEach(c => { c.style.display = ""; });
        root.querySelectorAll("[tag='description']").forEach(c => { c.style.display = "none"; });

        root.querySelectorAll("div[data-schematype='array']").forEach(c => {
            var items = c.querySelectorAll("button[tag='del']");
            if (0 == items.length) {
                var p = c.querySelector("div[tag='input']");
                if (null !== p) {
                    p.style.display = "none";
                }
            }
            else if (1 < items.length) {
                var b = c.querySelector("button[tag='array']");
                if (null !== b) {
                    b.style.display = "";
                }
            }
        });
    }

    //root.querySelectorAll("[tag='input']")
}

function ready(editor) {
    editor.on('ready', function () {
        //console.time("timer");
        var input = editor.input;
        var hasObject = input.editors.d && ("object" === input.schema.properties.d.type || "array" === input.schema.properties.d.type);
        var data = getData(input, false);
        if (input.schema.argSingle && !input.editors.d) {
            input.schema.data = { t: data.t };
        }
        else if (input.schema.argSingle && input.editors.d && !hasObject) {
            input.schema.data = { t: data.t, d: data.d };
        }
        else {
            input.schema.data = { t: data.t, d: JSON.parse(data.d) };
        }

        if (input.editors.t || input.editors.d) {
            var buttonEdit = editor.root.getButton('', 'edit', 'Edit');
            button_holder = editor.root.theme.getHeaderButtonHolder();
            button_holder.appendChild(buttonEdit);
            editor.root.header.parentNode.appendChild(button_holder);
            buttonEdit.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                setEdit(input.container, input.schema.edit);
                input.schema.edit = !input.schema.edit;
            }, false);
        }

        if (input.editors.d && !(input.schema.argSingle && !hasObject)) {
            var buttonData = editor.root.getButton('', 'data', 'Data');
            buttonData.id = "data";
            button_holder = input.editors.d.theme.getHeaderButtonHolder();
            button_holder.appendChild(buttonData);
            input.editors.d.header.parentNode.appendChild(button_holder);
            //this.editjson_controls.appendChild(this.buttonData);
            buttonData.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                //editor.root.editors.d.container.hidden = true; test
                input.editors.d.toggleEditJSON();
            }, false);

            if (input.editors.d.editjson_textarea) {
                input.editors.d.editjson_textarea.removeAttribute('tag');
                input.editors.d.editjson_textarea.style.display = '';
                //editor.root.editors.d.editjson_save.style.display = 'none';
            }
        }
        //console.timeEnd("timer");
        //======================================================//
        var debug = editor.sdkeditor;

        var curlValue = debug.editors["root.Curl.value"];
        var javascriptValue = debug.editors["root.JavaScript.value"];
        var netValue = debug.editors["root.NET.value"];
        var javaValue = debug.editors["root.Java.value"];
        var phpValue = debug.editors["root.PHP.value"];
        var debugValue = debug.editors["root.Debug.value"];

        var tab = debug.root.container.querySelector("a[aria-controls='Debug']");
        var collapse = debug.root_container.querySelector("button[tag='collapse']");

        var header = debug.root.header.parentNode;

        if (doc[businessName].config.debug) {
            var buttonDebug = debug.root.getButton('', 'debug', 'Debug');
            button_holder = debug.root.theme.getHeaderButtonHolder();
            button_holder.appendChild(buttonDebug);
            header.appendChild(button_holder);
            buttonDebug.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();

                debugValue.setValue(null);
                $trigger(tab, 'click');
                if ("Expand" === collapse.getAttribute("title")) {
                    $trigger(collapse, 'click');
                }

                if (input.editors.f) {
                    var form = new FormData();
                    var data = getData(input, false);
                    form.append("c", input.schema.name);
                    if (data.hasOwnProperty("t")) {
                        form.append("t", data.t);
                    }
                    if (data.hasOwnProperty("d")) {
                        form.append("d", data.d);
                    }

                    var files = input.container.querySelectorAll("[tag='file']");
                    files.forEach(c => {
                        form.append(c.file.name, c.file);
                    });

                    ajax.postForm(doc[businessName].config.host + "/" + businessName,
                        form,
                        function (response) {
                            //succcess
                            try {
                                debugValue.setValue(JSON.stringify(JSON.parse(response), null, 4));
                            } catch (e) {
                                debugValue.setValue(response);
                            }
                        }, function (response) {
                            //fail
                            debugValue.setValue(response.responseText);
                        });
                }
                else {
                    var data = getData(input, false);
                    var d = { c: input.schema.name };
                    if (data.hasOwnProperty("t")) {
                        d.t = data.t;
                    }
                    if (data.hasOwnProperty("d")) {
                        d.d = data.d;
                    }
                    ajax.post(doc[businessName].config.host + "/" + businessName, d,
                        function (response) {
                            //succcess
                            try {
                                debugValue.setValue(JSON.stringify(JSON.parse(response), null, 4));
                            } catch (e) {
                                debugValue.setValue(response);
                            }
                        }, function (response) {
                            //fail
                            debugValue.setValue(response);
                        });
                }
            }, false);
        }

        button_holder = debug.root.theme.getHeaderButtonHolder();
        button_holder.innerHTML = compiled_benchmark.render({ name: editor.schema.properties.input.name });
        //button_holder.classList.add("input-group");
        //header.appendChild(button_holder);

        header.parentNode.removeChild(header.nextSibling);
        var header2 = document.createElement('div');
        header2.classList.add("form-inline");
        header2.style.display = 'none';
        header2.style.marginTop = '8px';
        header.parentNode.insertBefore(header2, header.nextSibling);

        if (doc[businessName].config.testing) {
            var testing = button_holder.querySelector('#' + editor.schema.properties.input.name + '_testing');
            //var testing_btn = button_holder.querySelector('#' + editor.schema.properties.input.name + '_testing_btn');
            //var testingAll_btn = button_holder.querySelector('#' + editor.schema.properties.input.name + '_testingAll_btn');
            header2.appendChild(testing);
            //header2.appendChild(testing_btn);
            //header2.appendChild(testingAll_btn);

            var tests = []
            for (var i in input.schema.testing) {
                tests.push(i);
            }
            for (var i = tests.length - 1; i >= 0; i--) {
                testing.options.add(new Option(tests[i], tests[i]));
            }

            testing.addEventListener('change', function (el) {
                if (null == el.currentTarget) {
                    return;
                }
                var obj = el.currentTarget;
                if (-1 !== obj.selectedIndex) {
                    setData(input, input.schema.data.d, 0 === obj.selectedIndex);
                    if (0 === obj.selectedIndex) {
                        return;
                    }
                    var select = obj.options[obj.selectedIndex];
                    var value = JSON.parse(input.schema.testing[select.value].value);
                    setData(input, value);
                }
            }, false);

            header2.style.display = "";
        }

        if (doc[businessName].config.benchmark) {
            header.classList.add("form-inline");
            var buttonBenchmark = button_holder.querySelector('#' + editor.schema.properties.input.name + '_benchmark');
            var benchmark_n = button_holder.querySelector('#' + editor.schema.properties.input.name + '_benchmark_n');
            var benchmark_c = button_holder.querySelector('#' + editor.schema.properties.input.name + '_benchmark_c');
            header.appendChild(buttonBenchmark);
            header.appendChild(benchmark_n);
            header.appendChild(benchmark_c);
            buttonBenchmark.addEventListener('click', function (e) {
                buttonBenchmark.setAttribute("disabled", true);
                e.preventDefault();
                e.stopPropagation();

                try {
                    debugValue.setValue(null);
                    $trigger(tab, 'click');
                    if ("Expand" === collapse.getAttribute("title")) {
                        $trigger(collapse, 'click');
                    }

                    var n = Number(benchmark_n.value);
                    if (isNaN(n) || 0 >= n) {
                        debugValue.setValue('Request numerical must be greater than 0!');
                        buttonBenchmark.removeAttribute("disabled");
                        return;
                    }

                    var c = Number(benchmark_c.value);
                    if (isNaN(c) || 0 >= c) {
                        debugValue.setValue('Concurrency numerical must be greater than 0!');
                        buttonBenchmark.removeAttribute("disabled");
                        return;
                    }

                    if (c > n) {
                        debugValue.setValue('Concurrency numerical cannot be greater than Request numerical!');
                        buttonBenchmark.removeAttribute("disabled");
                        return;
                    }

                    var data = getData(input, false);
                    //input.schema.name
                    ajax.post(doc[businessName].config.host + "/" + businessName,
                        {
                            c: "benchmark",
                            t: null,//token check
                            d: JSON.stringify({
                                n: n,
                                c: c,
                                data: "c=" + input.schema.name + "&t=" + data.t + "&d=" + (doc[businessName].config.benchmarkJSON ? JSON.stringify(data.d) : data.d),
                                host: doc[businessName].config.host + "/" + businessName
                            })
                        },
                        function (response) {
                            //succcess
                            buttonBenchmark.removeAttribute("disabled");
                            debugValue.setValue(response);
                        }, function (response) {
                            //fail
                            buttonBenchmark.removeAttribute("disabled");
                            debugValue.setValue(response);
                        });
                } catch (e) {
                    buttonBenchmark.removeAttribute("disabled");
                    debugValue.setValue(e);
                }
            }, false);
        }

        if (doc[businessName].config.setToken) {
            var token = button_holder.querySelector('#' + editor.schema.properties.input.name + '_token');
            var settoken = button_holder.querySelector('#' + editor.schema.properties.input.name + '_settoken');
            header.appendChild(settoken);
            header.appendChild(token);
            settoken.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();

                tokenvalue = token.value;
                var t = document.getElementsByName("root[input][t]");
                for (var i = 0; i < t.length; i++) {
                    t[i].value = tokenvalue;
                    $trigger(t[i], 'change');
                }
            }, false);
        }

        //======================================================//

        editor.on('change', function () {
            var input = this.editors["root.input"];
            var data = getData(input, false);
            var h = doc[businessName].config.host + "/" + businessName;

            curlValue.setValue(GetCurl(h, input.schema.name, data, getData2(input, false)));
            javascriptValue.setValue(GetSdkJavaScript(h, input.schema.name, data));
            netValue.setValue(GetSdkNet(h, input.schema.name, data));

            //console.log(this.sdkeditor.getValue());
        });
        //console.timeEnd("timer");
    });
}

function setSdk(sdk) {
    if (null == sdk) { return; }

    sdk.querySelector('#Curl > div > div > h4').style.display = 'none';
    sdk.querySelector('#JavaScript > div > div > h4').style.display = 'none';
    sdk.querySelector('#NET > div > div > h4').style.display = 'none';
    sdk.querySelector('#Java > div > div > h4').style.display = 'none';
    sdk.querySelector('#PHP > div > div > h4').style.display = 'none';
    sdk.querySelector('#Debug > div > div > h4').style.display = 'none';

    sdk.querySelector('#Curl > div > div > div.well.well-sm > div > div > div > div > div.form-group > label').style.display = 'none';
    sdk.querySelector('#JavaScript > div > div > div.well.well-sm > div > div > div > div > div.form-group > label').style.display = 'none';
    sdk.querySelector('#NET > div > div > div.well.well-sm > div > div > div > div > div.form-group > label').style.display = 'none';
    sdk.querySelector('#Java > div > div > div.well.well-sm > div > div > div > div > div.form-group > label').style.display = 'none';
    sdk.querySelector('#PHP > div > div > div.well.well-sm > div > div > div > div > div.form-group > label').style.display = 'none';
    sdk.querySelector('#Debug > div > div > div.well.well-sm > div > div > div > div > div.form-group > label').style.display = 'none';

    //var sdk = sdkeditor.root.container.querySelector("div[id='SDK & Debug']");
    sdk.style.padding = sdk.style.margin = sdk.style.border = "0px";

    sdk.querySelector('#Curl > div > div').removeAttribute("class");
    var panel = sdk.querySelector("#Curl > div > div > div[tag='panel']");
    panel.removeAttribute("class"); panel.removeAttribute("style");
    var row = sdk.querySelector("#Curl > div > div > div[tag='panel'] > div > div > div[tag='row']");
    row.removeAttribute("class"); row.removeAttribute("style");
    sdk.querySelector("#Curl > div > div > div[tag='panel'] > div > div > div[tag='row'] > div").classList.add("emacs-mode");

    sdk.querySelector('#JavaScript > div > div').removeAttribute("class");
    var panel = sdk.querySelector("#JavaScript > div > div > div[tag='panel']");
    panel.removeAttribute("class"); panel.removeAttribute("style");
    var row = sdk.querySelector("#JavaScript > div > div > div[tag='panel'] > div > div > div[tag='row']");
    row.removeAttribute("class"); row.removeAttribute("style");
    sdk.querySelector("#JavaScript > div > div > div[tag='panel'] > div > div > div[tag='row'] > div").classList.add("emacs-mode");

    sdk.querySelector('#NET > div > div').removeAttribute("class");
    var panel = sdk.querySelector("#NET > div > div > div[tag='panel']");
    panel.removeAttribute("class"); panel.removeAttribute("style");
    var row = sdk.querySelector("#NET > div > div > div[tag='panel'] > div > div > div[tag='row']");
    row.removeAttribute("class"); row.removeAttribute("style");
    sdk.querySelector("#NET > div > div > div[tag='panel'] > div > div > div[tag='row'] > div").classList.add("emacs-mode");

    sdk.querySelector('#Java > div > div').removeAttribute("class");
    var panel = sdk.querySelector("#Java > div > div > div[tag='panel']");
    panel.removeAttribute("class"); panel.removeAttribute("style");
    var row = sdk.querySelector("#Java > div > div > div[tag='panel'] > div > div > div[tag='row']");
    row.removeAttribute("class"); row.removeAttribute("style");
    sdk.querySelector("#Java > div > div > div[tag='panel'] > div > div > div[tag='row'] > div").classList.add("emacs-mode");

    sdk.querySelector('#PHP > div > div').removeAttribute("class");
    var panel = sdk.querySelector("#PHP > div > div > div[tag='panel']");
    panel.removeAttribute("class"); panel.removeAttribute("style");
    var row = sdk.querySelector("#PHP > div > div > div[tag='panel'] > div > div > div[tag='row']");
    row.removeAttribute("class"); row.removeAttribute("style");
    sdk.querySelector("#PHP > div > div > div[tag='panel'] > div > div > div[tag='row'] > div").classList.add("emacs-mode");

    sdk.querySelector('#Debug > div > div').removeAttribute("class");
    var panel = sdk.querySelector("#Debug > div > div > div[tag='panel']");
    panel.removeAttribute("class"); panel.removeAttribute("style");
    var row = sdk.querySelector("#Debug > div > div > div[tag='panel'] > div > div > div[tag='row']");
    row.removeAttribute("class"); row.removeAttribute("style");
    sdk.querySelector("#Debug > div > div > div[tag='panel'] > div > div > div[tag='row'] > div").classList.add("emacs-mode");
}

function getSdk(format) {
    return {
        type: "object",
        properties: {
            value: {
                type: "string",
                format: format,
                options: {
                    ace: {
                        theme: "ace/theme/vibrant_ink",
                        wrap: true,
                        useWorker: false,
                        tabSize: 4,
                        readOnly: true,
                        highlightActiveLine: false,
                        highlightSelectedWord: false,
                        minLines: 25,
                        maxLines: 25
                    }
                }
            }
        },
        options: {
            collapsed: false,
            disable_collapse: true
        }
    };
}

function getData(input, format = true) {
    var data = {};
    if (input.editors.d) {
        var d = null;
        if (input.schema.argSingle) {
            if (input.schema.argSingle && "object" !== input.schema.properties.d.type && "array" !== input.schema.properties.d.type) {
                d = input.editors.d.getValue();
            }
            else {
                d = JSON.stringify(input.editors.d.getValue(), null, format ? 2 : 0);
            }
        }
        else {
            var args = []
            for (var i in input.editors.d.editors) {
                args.push(i);
            }
            for (var i = args.length - 1; i >= 0; i--) {
                args[i] = input.editors.d.editors[args[i]].getValue();
            }
            d = JSON.stringify(args, null, format ? 2 : 0);
        }
        data.d = d;
    }

    if (input.editors.t) {
        data.t = input.editors.t.getValue();
    }

    return data;
}

function getData2(input, format = true) {
    var data = {};
    if (input.editors.d) {
        if (input.schema.argSingle) {
            if (input.schema.argSingle && "object" !== input.schema.properties.d.type && "array" !== input.schema.properties.d.type) {
                d = input.editors.d.schema.name + "=" + encodeURIComponent(input.editors.d.getValue());
            }
            else {
                d = input.editors.d.schema.name + "=" + encodeURIComponent(JSON.stringify(input.editors.d.getValue(), null, format ? 2 : 0));
            }
            data.d = d;
        }
        else {
            var args = []
            for (var i in input.editors.d.editors) {
                args.push(i);
            }
            for (var i = args.length - 1; i >= 0; i--) {
                if ("object" !== input.editors.d.editors[args[i]].schema.type && "array" !== input.editors.d.editors[args[i]].schema.type) {
                    args[i] = args[i] + "=" + encodeURIComponent(input.editors.d.editors[args[i]].getValue());
                }
                else {
                    args[i] = args[i] + "=" + encodeURIComponent(JSON.stringify(input.editors.d.editors[args[i]].getValue(), null, format ? 2 : 0));
                }
            }
            data.d = args.join("&");
        }
    }

    if (input.editors.t) {
        data.t = input.editors.t.getValue();
    }

    return data;
}

function setData(input, value, refresh = true) {
    if (input.editors.d) {
        if (input.schema.argSingle) {
            return input.editors.d.setValue(value);
        }
        else {
            var args = []
            for (var i in input.editors.d.editors) {
                args.push(i);
            }
            for (var i = 0; i < args.length; i++) {
                if (i < value.length) {
                    input.editors.d.editors[args[i]].setValue(value[i]);
                }
            }
        }
    }
    if (refresh) {
        input.editors.d.onChange(true);
    }
}

go();