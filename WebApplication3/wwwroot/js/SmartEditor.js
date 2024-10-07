let oEditors = [];

smartEditor = function () {
    nhn.husky.EZCreator.createInIFrame({
        oAppRef: oEditors,
        elPlaceHolder: "editorTxt",
        sSkinURI: "/lib/smarteditor2/SmartEditor2Skin.html",
        fCreator: "createSEditor2"
    });
}

function stripHtmlTags(html) {
    var tempDiv = document.createElement("div");
    tempDiv.innerHTML = html.replace(/<\/p>/gi, "\r\n")     // </p> 태그를 줄바꿈으로 대체
        .replace(/<[^>]+>/g, '');     // 나머지 HTML 태그는 모두 제거
    console.log(tempDiv)

    // HTML 엔티티를 일반 문자로 변환
    var text = tempDiv.textContent || tempDiv.innerText || "";
    console.log(text)
    return text;
}



function submitForm() {
    oEditors.getById["editorTxt"].exec("UPDATE_CONTENTS_FIELD", []);
    const contentHtml = document.querySelector("textarea[name='Content']").value;

    // HTML 태그를 제거하여 일반 텍스트로 변환
    //const contentText = stripHtmlTags(contentHtml);
    
    document.querySelector("textarea[name='Content']").value = contentHtml;
    document.getElementById("qnaForm").submit();
}

function pr() {
    oEditors.getById["editorTxt"].exec("UPDATE_CONTENTS_FIELD", []);
    const contentHtml = document.querySelector("textarea[name='Content']").value;

    // HTML 태그를 제거하여 일반 텍스트로 변환
    console.log(stripHtmlTags(contentHtml));
}