$(function () {

    $('#progressbar').hide();
    $('#progressSwitch').on('click', function () {
        $('#progressbar').show();
    });

    $('#uploadFile').on('click', function () {
        const file = $('#file')[0].files[0];

        let formData = new FormData();
        formData.append('file', file);

        // You can abort the upload by calling jqxhr.abort();    
        $.ajax({
            url: "/import",
            type: "POST",
            contentType: false,
            data: formData,
            dataType: "json",
            cache: false,
            processData: false,
            xhr: function () {
                var xhr = new window.XMLHttpRequest();
                xhr.upload.addEventListener("progress",
                    function (evt) {
                        if (evt.lengthComputable) {
                            var progress = Math.round((evt.loaded / evt.total) * 100);

                            // Do something with the progress
                        }
                    },
                    false);
                return xhr;
            }
        })
            .done(function (data, textStatus, jqXhr) {
                console.log(data);

                // Clear the input
                $("#file").val('');
                alert(data.msg);
            })
            .fail(function (jqXhr, textStatus, errorThrown) {
                if (errorThrown === "abort") {
                    alert("Uploading was aborted");
                } else {
                    alert("Uploading failed");
                }
            })
            .always(function (data, textStatus, jqXhr) { });

    });

    $('#uploadFileSync').on('click', function () {
        const file = $('#fileSync')[0].files[0];

        let formData = new FormData();
        formData.append('file', file);

        // You can abort the upload by calling jqxhr.abort();    
        $.ajax({
            url: "/import-sync",
            type: "POST",
            contentType: false,
            data: formData,
            dataType: "json",
            cache: false,
            processData: false,
            xhr: function () {
                var xhr = new window.XMLHttpRequest();
                xhr.upload.addEventListener("progress",
                    function (evt) {
                        if (evt.lengthComputable) {
                            var progress = Math.round((evt.loaded / evt.total) * 100);

                            // Do something with the progress
                        }
                    },
                    false);
                return xhr;
            }
        })
            .done(function (data, textStatus, jqXhr) {
                console.log(data);

                // Clear the input
                $("#fileSync").val('');
                alert(data.msg);
            })
            .fail(function (jqXhr, textStatus, errorThrown) {
                if (errorThrown === "abort") {
                    alert("Uploading was aborted");
                } else {
                    alert("Uploading failed");
                }
            })
            .always(function (data, textStatus, jqXhr) { });

    });

    $('#uploadFileSyncNpoi').on('click', function () {
        const file = $('#fileSyncNpoi')[0].files[0];

        let formData = new FormData();
        formData.append('file', file);

        // You can abort the upload by calling jqxhr.abort();    
        $.ajax({
            url: "/import-sync-npoi",
            type: "POST",
            contentType: false,
            data: formData,
            dataType: "json",
            cache: false,
            processData: false,
            xhr: function () {
                var xhr = new window.XMLHttpRequest();
                xhr.upload.addEventListener("progress",
                    function (evt) {
                        if (evt.lengthComputable) {
                            var progress = Math.round((evt.loaded / evt.total) * 100);

                            // Do something with the progress
                        }
                    },
                    false);
                return xhr;
            }
        })
            .done(function (data, textStatus, jqXhr) {
                console.log(data);

                // Clear the input
                $("#fileSyncNpoi").val('');
                alert(data.msg);
            })
            .fail(function (jqXhr, textStatus, errorThrown) {
                if (errorThrown === "abort") {
                    alert("Uploading was aborted");
                } else {
                    alert("Uploading failed");
                }
            })
            .always(function (data, textStatus, jqXhr) { });

    });
});