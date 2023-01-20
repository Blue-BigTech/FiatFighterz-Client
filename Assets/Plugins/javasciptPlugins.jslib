mergeInto(LibraryManager.library, {
    Hello: function() {
        window.alert("Hello World!");
    },

    PostDataReturn: function(url, postdata){
        console.log(Pointer_stringify(url));

        var encodedData = new URLSearchParams(postdata);

        try {
          jQuery.ajax({
              type: "POST",
              url: url,
              cache: false,
              data: postdata,
              success: function (msg){
                alert("success" + msg);
              },
              error: function (msg){
                alert("error" + msg);
              }
            });
          } catch(error) {
            window.alert(error);
          }
        return postdata;
    }
});
