module Roadkill.Web {
    /**
	Additional, dynamically loaded buttons for the toolbar
	*/
    export interface IWysiwygButton {
        id: string;
        clickAction(e: any, parent: WysiwygEditor): void;
    }
} 