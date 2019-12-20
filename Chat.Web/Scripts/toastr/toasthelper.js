(function ($) {
    showSuccessToast = function (heading,msg) {
        'use strict';
        resetToastPosition();
        $.toast({
            heading: heading,
            text: msg,
            showHideTransition: 'slide',
            icon: 'success',
            loaderBg: '#4ac176',
            position: 'top-right'
        })
    };
    showInfoToast = function (heading, msg) {
        'use strict';
        resetToastPosition();
        $.toast({
            heading: heading,
            text: msg,
            showHideTransition: 'slide',
            icon: 'info',
            loaderBg: '#46c35f',
            position: 'top-right'
        })
    };
    showWarningToast = function (heading, msg) {
        'use strict';
        resetToastPosition();
        $.toast({
            heading: heading,
            text: msg,
            showHideTransition: 'slide',
            icon: 'warning',
            loaderBg: '#57c7d4',
            position: 'top-right'
        })
    };
    showDangerToast = function (heading, msg) {
        'use strict';
        resetToastPosition();
        $.toast({
            heading: heading,
            text: msg,
            showHideTransition: 'slide',
            icon: 'error',
            loaderBg: '#f2a654',
            position: 'top-right'
        })
    };
    showToastPosition = function (heading, msg,position) {
        'use strict';
        resetToastPosition();
        $.toast({
            heading: heading,
            text: msg,
            position: String(position),
            icon: 'info',
            stack: false,
            loaderBg: '#f96868'
        })
    }
    resetToastPosition = function () {
        $('.jq-toast-wrap').removeClass('bottom-left bottom-right top-left top-right mid-center'); // to remove previous position class
        $(".jq-toast-wrap").css({
            "top": "",
            "left": "",
            "bottom": "",
            "right": ""
        }); 
    }
})(jQuery);