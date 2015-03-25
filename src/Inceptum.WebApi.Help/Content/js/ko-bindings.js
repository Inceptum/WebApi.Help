; (function (window, ko) {

    // Code highlighter binding
    ko.bindingHandlers.code = {
        init: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
            // NOP 
        },
        update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {            
            var codeParams = valueAccessor(), // object = { value: value, syntax: 'json' }
                val = codeParams.value,
                formatter = ko.bindingHandlers.code.formatters[codeParams.syntax || 'json'];
            if (typeof (val) != "string")
                val = JSON.stringify(val, null, '\t');

            $(element).html(formatter ? formatter(val) : val);
        },
        formatters: {
            'json': function (text) {
                var json = text.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
                return json.replace(/("(\\u[a-zA-Z0-9]{4}|\\[^u]|[^\\"])*"(\s*:)?|\b(true|false|null)\b|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?)/g, function (match) {
                    var cls = 'json number';
                    if (/^"/.test(match)) {
                        if (/:$/.test(match)) {
                            cls = 'json key';
                        } else {
                            cls = 'json string';
                        }
                    } else if (/true|false/.test(match)) {
                        cls = 'json boolean';
                    } else if (/null/.test(match)) {
                        cls = 'json null';
                    }
                    return '<span class="' + cls + '">' + match + '</span>';
                });
            }
        }
    };

})(window, ko);