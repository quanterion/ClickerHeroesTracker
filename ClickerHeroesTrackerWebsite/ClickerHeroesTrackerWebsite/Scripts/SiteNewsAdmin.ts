﻿module SiteNewsAdmin
{
    export function init(container: HTMLElement): void
    {
        if (!container)
        {
            throw new Error("Element not found: " + container);
        }

        var entries = container.children;
        for (var i = 0; i < entries.length; i++)
        {
            var entry = entries[i];
            var dateHeading = <HTMLElement>entry.querySelector("h3");

            var buttonContainer = document.createElement("div");
            buttonContainer.classList.add("pull-right");

            var editButton = document.createElement("button");
            editButton.appendChild(document.createTextNode("Edit"));
            editButton.addEventListener("click", editButtonClicked);
            buttonContainer.appendChild(editButton);

            var saveButton = document.createElement("button");
            saveButton.classList.add("hide");
            saveButton.appendChild(document.createTextNode("Save"));
            saveButton.addEventListener("click", saveButtonClicked);
            buttonContainer.appendChild(saveButton);

            var cancelButton = document.createElement("button");
            cancelButton.classList.add("hide");
            cancelButton.appendChild(document.createTextNode("Cancel"));
            cancelButton.addEventListener("click", cancelButtonClicked);
            buttonContainer.appendChild(cancelButton);

            var deleteButton = document.createElement("button");
            deleteButton.appendChild(document.createTextNode("Delete"));
            deleteButton.classList.add("pull-right");
            deleteButton.addEventListener("click", deleteButtonClicked);
            buttonContainer.appendChild(deleteButton);

            dateHeading.appendChild(buttonContainer);
        }

        var addButton = document.createElement("button");
        addButton.appendChild(document.createTextNode("Add"));
        addButton.addEventListener("click", addButtonClicked);
        container.insertBefore(addButton, container.firstChild);
    }

    function addButtonClicked(ev: MouseEvent): void
    {
        var addButton = <HTMLElement>ev.target;

        var dateContainer = document.createElement("div");
        dateContainer.setAttribute("data-date", "");

        var dateHeading = <HTMLElement>document.createElement("h3");
        var headingInput = document.createElement("input");
        headingInput.classList.add("input-md");
        headingInput.value = new Date().toLocaleDateString();
        dateHeading.appendChild(headingInput);

        var buttonContainer = document.createElement("div");
        buttonContainer.classList.add("pull-right");

        var saveButton = document.createElement("button");
        saveButton.appendChild(document.createTextNode("Save"));
        saveButton.addEventListener("click", saveButtonClicked);
        buttonContainer.appendChild(saveButton);

        var cancelButton = document.createElement("button");
        cancelButton.appendChild(document.createTextNode("Cancel"));
        cancelButton.addEventListener("click", cancelButtonClicked);
        buttonContainer.appendChild(cancelButton);

        dateHeading.appendChild(buttonContainer);
        dateContainer.appendChild(dateHeading);

        var list = document.createElement("ul");
        var listItem = document.createElement("li");
        var input = document.createElement("textarea");
        input.classList.add("form-control");
        input.style.maxWidth = "none";
        input.addEventListener("blur", inputBlurred);
        listItem.appendChild(input);
        list.appendChild(listItem);

        dateContainer.appendChild(list);
        addButton.parentElement.insertBefore(dateContainer, addButton.nextSibling);
    }

    function editButtonClicked(ev: MouseEvent): void
    {
        var container = getNewsEntityContainer(<HTMLElement>ev.target);
        if (!container)
        {
            return;
        }

        var heading = container.querySelector("h3");
        heading.setAttribute("data-original", heading.firstChild.nodeValue);
        var headingInput = document.createElement("input");
        headingInput.classList.add("input-md");
        headingInput.value = heading.firstChild.nodeValue;
        heading.replaceChild(headingInput, heading.firstChild);

        var list = container.querySelector("ul");
        var listItems = list.querySelectorAll("li");
        for (var i = 0; i < listItems.length; i++)
        {
            var listItem = <HTMLLIElement>listItems[i];
            listItem.setAttribute("data-original", listItem.innerHTML);

            var input = document.createElement("textarea");
            input.classList.add("form-control");
            input.style.maxWidth = "none";
            input.innerHTML = listItem.innerHTML;
            input.addEventListener("blur", inputBlurred);

            listItem.innerHTML = "";
            listItem.appendChild(input);
        }

        var listItem = document.createElement("li");
        var input = document.createElement("textarea");
        input.classList.add("form-control");
        input.style.maxWidth = "none";
        input.addEventListener("blur", inputBlurred);
        listItem.appendChild(input);
        list.appendChild(listItem);

        var buttons = container.querySelectorAll("button");
        for (var i = 0; i < buttons.length; i++)
        {
            buttons[i].classList.toggle("hide");
        }
    }

    function saveButtonClicked(ev: MouseEvent): void
    {
        var container = getNewsEntityContainer(<HTMLElement>ev.target);
        if (!container)
        {
            return;
        }

        var originalDateStr = container.getAttribute("data-date");

        var heading = container.querySelector("h3");
        var headingInput = <HTMLInputElement>heading.firstChild;
        var milliseconds = Date.parse(headingInput.value);
        if (isNaN(milliseconds))
        {
            alert("Couldn't parse the date");
            return;
        }

        // The date will be parsed as local time, so we need to convert to UTC by subtracting out the timezone offset used on that day.
        var date = new Date(milliseconds);
        var dateUtcStr = new Date(date.getTime() - (date.getTimezoneOffset() * 60 * 1000)).toJSON();

        var headingText = document.createTextNode(date.toLocaleDateString());
        heading.replaceChild(headingText, headingInput);

        var messages: string[] = [];
        var listItems = container.querySelectorAll("li");
        for (var i = 0; i < listItems.length; i++)
        {
            var listItem = listItems[i];
            var input = <HTMLInputElement>listItem.firstChild;
            var message = input.value.trim();
            if (message)
            {
                messages.push(message);
                listItem.innerHTML = input.value;
                listItem.removeAttribute("data-original");
            }
            else
            {
                listItem.remove();
            }
        }

        // If the date changed, we need to delete first
        if (originalDateStr && dateUtcStr != originalDateStr)
        {
            var buttons = container.querySelectorAll("button");
            for (var i = 0; i < buttons.length; i++)
            {
                buttons[i].setAttribute("disabled", "disabled");
            }

            $.ajax({
                url: '/api/news/' + originalDateStr.substring(0, 10),
                type: 'delete'
            })
                .done((response: ISiteNewsEntryListResponse) =>
                {
                })
                .fail(() =>
                {
                    alert("Something bad happened when deleting the old entry");
                    debugger;
                });
        }

        var buttons = container.querySelectorAll("button");
        for (var i = 0; i < buttons.length; i++)
        {
            buttons[i].setAttribute("disabled", "disabled");
        }

        var data =
            {
                date: dateUtcStr.substring(0, 10),
                messages: messages
            };

        $.ajax({
            url: '/api/news',
            type: 'post',
            data: data
        })
            .done((response: ISiteNewsEntryListResponse) =>
            {
                var buttons = container.querySelectorAll("button");
                for (var i = 0; i < buttons.length; i++)
                {
                    buttons[i].removeAttribute("disabled");
                    buttons[i].classList.toggle("hide");
                }
            })
            .fail(() =>
            {
                alert("Something bad happened inserting the new entry");
                debugger;
            });
    }

    function cancelButtonClicked(ev: MouseEvent): void
    {
        var container = getNewsEntityContainer(<HTMLElement>ev.target);
        if (!container)
        {
            return;
        }

        var heading = container.querySelector("h3");

        if (!heading.hasAttribute("data-original"))
        {
            container.remove();
            return;
        }

        var headingText = document.createTextNode(heading.getAttribute("data-original"));
        heading.replaceChild(headingText, heading.firstChild);

        var listItems = container.querySelectorAll("li");
        for (var i = 0; i < listItems.length; i++)
        {
            var listItem = listItems[i];
            if (!listItem.hasAttribute("data-original"))
            {
                listItem.remove();
                continue;
            }

            listItem.innerHTML = listItem.getAttribute("data-original");
            listItem.removeAttribute("data-original");
        }

        var buttons = container.querySelectorAll("button");
        for (var i = 0; i < buttons.length; i++)
        {
            buttons[i].classList.toggle("hide");
        }
    }

    function deleteButtonClicked(ev: MouseEvent): void
    {
        var container = getNewsEntityContainer(<HTMLElement>ev.target);
        if (!container)
        {
            return;
        }

        var dateStr = container.getAttribute("data-date");

        var buttons = container.querySelectorAll("button");
        for (var i = 0; i < buttons.length; i++)
        {
            buttons[i].setAttribute("disabled", "disabled");
        }

        $.ajax({
            url: '/api/news/' + dateStr.substring(0, 10),
            type: 'delete'
        })
            .done((response: ISiteNewsEntryListResponse) =>
            {
                container.remove();
            })
            .fail(() =>
            {
                alert("Something bad happened");
                debugger;
            });
    }

    function inputBlurred(ev: FocusEvent): void
    {
        var blurredInput = <HTMLTextAreaElement>ev.target;
        var isLast = blurredInput.parentElement.nextSibling == null;
        var isEmpty = blurredInput.value.trim().length == 0;

        if (isLast && !isEmpty)
        {
            var listItem = document.createElement("li");
            var input = document.createElement("textarea");
            input.classList.add("form-control");
            input.style.maxWidth = "none";
            input.addEventListener("blur", inputBlurred);
            listItem.appendChild(input);
            blurredInput.parentElement.parentElement.appendChild(listItem);
        }
        else if (!isLast && isEmpty)
        {
            blurredInput.parentElement.remove();
        }
    }

    function getNewsEntityContainer(element: HTMLElement)
    {
        var container = element;
        while (container && !container.hasAttribute("data-date"))
        {
            container = container.parentElement;
        }

        return container;
    }
}