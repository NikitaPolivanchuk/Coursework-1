const autoCompleteJS = new autoComplete({
            placeHolder: "Search...",
            data: {
                src: async () => {
                    const source = await fetch("../../Home/GetHints", {method: 'get'});
                    const data = await source.text();
                    return data.split("&");
                },
                cache: true
            },

            resultItem: {
                highlight: true,
                tabSelect: true
            },
            events: {
                input: {
                    selection: (event) => {
                        const selection = event.detail.selection.value;
                        autoCompleteJS.input.value = selection;
                    }
                }
            }
        });