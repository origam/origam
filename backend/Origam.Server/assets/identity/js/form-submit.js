
const form = document.querySelector("form");
const submitLinks = document.getElementsByClassName("submit-link");

Array.prototype.forEach.call(
    submitLinks, element => {
        element.addEventListener("click",function(event){
            const input = document.createElement("input");
            input.type = "hidden"
            input.name = "button"
            input.value = element.getAttribute("value");
            form.append(input)
            form.submit();
            event.preventDefault();
        });
});

const languageForm = document.getElementById("languageSelectForm");
const languageLinks = document.getElementsByClassName("languageLink");
Array.prototype.forEach.call(
    languageLinks, element => {
        element.addEventListener("click",function(event){
            const input = document.createElement("input");
            input.type = "hidden"
            input.name = "culture"
            input.value = element.getAttribute("value");
            languageForm.append(input)
            languageForm.submit();
            event.preventDefault();
        });
});


function onLoginEnter(e) {
    if (e.key === "Enter") {
        document.getElementById('loginButton').click()
    }
}

const passwordElement = document.getElementById('passInput');
if (passwordElement) {
    passwordElement.onkeydown = onLoginEnter;
}

const userElement = document.getElementById('userNameInput');
if (userElement) {
    userElement.onkeydown = onLoginEnter;
}


