
/* Add extension */
function readURL(input) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
            $('#blah').attr('src', e.target.result);
            document.getElementById("blah").hidden = false;
        }

        reader.readAsDataURL(input.files[0]);
    }
}
$("#filePhoto").change(function () {
    $('#subfile').val($(this).val());
    readURL(this);
});

