var tryNumber = 0;
jQuery('input[type=submit]').click(function (event) {
    var self = $(this);

    if (self.closest('form').valid()) {
        if (tryNumber > 0) {
            tryNumber++;
            // alert('Your form has been already submited. wait please'); 
            return false;
        }
        else {
            tryNumber++;
        }
    };

});
