/*

Copyright (c) 2009 Stefano J. Attardi, http://attardi.org/

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

*/
(function ($) {
    function toggleLabel() {
        var input = $(this);
        setTimeout(function () {
            var def = input.attr('title');
            if (!input.val() || (input.val() == def)) {
                input.prev('span').css('visibility', '');
                if (def) {
                    var dummy = $('<label></label>').text(def).css('visibility', 'hidden').appendTo('body');
                    input.prev('span').css('margin-left', dummy.width() + 3 + 'px');
                    dummy.remove();
                }
            } else {
                input.prev('span').css('visibility', 'hidden');
            }
        }, 0);

    };

    // Call toggleLabel for more than one html elements
    // e.g. call toggleLable for username and password fields
    function toggleLabels(objects) {
        for (var i = 0; i < objects.data.length; i++) {
            toggleLabel.call(objects.data[i]);
        }
    }

    function resetField() {
        var def = $(this).attr('title');
        if (!$(this).val() || ($(this).val() == def)) {
            $(this).val(def);
            $(this).prev('span').css('visibility', '');
        }
    };

    uname = $('#UserName');
    passwd = $('#Password');
    both = $('#UserName, #Password');
    recover = $('#Email')
    code = $('#ConfirmationCode');
    lastName = $('#Name');
    firstName = $('#FirstName');

    // keydown
    // when username is pressed, handle both username and password,
    // otherwise browser autofill destroy password when known username is typed
    uname.on('keydown', [passwd, uname], toggleLabels);
    passwd.on('keydown', toggleLabel);
    recover.on('keydown', toggleLabel);
    code.on('keydown', toggleLabel);
    lastName.on('keydown', toggleLabel);
    firstName.on('keydown', toggleLabel);

    // paste
    uname.on('paste', [passwd, uname], toggleLabels);
    passwd.on('paste', toggleLabel);
    recover.on('paste', toggleLabel);
    code.on('paste', toggleLabel);
    lastName.on('paste', toggleLabel);
    firstName.on('paste', toggleLabel);

    // input
    /* both.on('input', function () {
    setTimeout(function () {                   
    both.each(function () { toggleLabel.call(this); });            
    }, 0);
    }); */
    uname.on('input', [passwd, uname], toggleLabels);
    passwd.on('input', toggleLabel);
    recover.on('input', toggleLabel);
    code.on('input', toggleLabel);
    lastName.on('input', toggleLabel);
    firstName.on('input', toggleLabel);


    // focusin
    both.on('focusin', function () {
        $(this).prev('span').css('color', '#ccc');
        both.each(function () { toggleLabel.call(this); });
    });
    recover.on('focusin', function () {
        $(this).prev('span').css('color', '#ccc');
        recover.each(function () { toggleLabel.call(this); });
    });
    code.on('focusin', function () {
        $(this).prev('span').css('color', '#ccc');
        code.each(function () { toggleLabel.call(this); });
    });
    lastName.on('focusin', function () {
        $(this).prev('span').css('color', '#ccc');
        lastName.each(function () { toggleLabel.call(this); });
    });
    firstName.on('focusin', function () {
        $(this).prev('span').css('color', '#ccc');
        firstName.each(function () { toggleLabel.call(this); });
    });
    // focusout
    both.on('focusout', function () {
        $(this).prev('span').css('color', '#999');
        both.each(function () { toggleLabel.call(this); });
    });
    recover.on('focusout', function () {
        $(this).prev('span').css('color', '#999');
        recover.each(function () { toggleLabel.call(this); });
    });
    code.on('focusout', function () {
        $(this).prev('span').css('color', '#999');
        code.each(function () { toggleLabel.call(this); });
    });
    lastName.on('focusout', function () {
        $(this).prev('span').css('color', '#999');
        lastName.each(function () { toggleLabel.call(this); });
    });
    firstName.on('focusout', function () {
        $(this).prev('span').css('color', '#999');
        firstName.each(function () { toggleLabel.call(this); });
    });

    $(function () {
        both.each(function () { toggleLabel.call(this); });
        recover.each(function () { toggleLabel.call(this); });
        code.each(function () { toggleLabel.call(this); });
        lastName.each(function () { toggleLabel.call(this); });
        firstName.each(function () { toggleLabel.call(this); });
    });


    // do it again to detect Chrome autofill
    $(window).load(function () {
        setTimeout(function () {
            both.each(function () { toggleLabel.call(this); });
            recover.each(function () { toggleLabel.call(this); });
        }, 0);
        code.focus();
        recover.focus();
        uname.focus();
        lastName.focus();
        firstName.focus();
    });

})(jQuery);
