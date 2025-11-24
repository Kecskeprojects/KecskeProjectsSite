import type { JSX } from "react";
import React from "react";
import type { IWindowDescription } from "../../interface/IWindowDescription";
import ResponseObject from "../../models/ResponseObject";

export type EditWindowBaseProps = {
  windowDescription: IWindowDescription;
  responseHandler: (content?: any) => void;
};

export default function EditWindowBase(
  props: EditWindowBaseProps
): JSX.Element {
  function submitHandler(e: React.FormEvent<HTMLFormElement>): void {
    e.preventDefault();
    const formData = new FormData(e.currentTarget);

    props.windowDescription
      .serviceFunction(formData)
      .then((res) => {
        if (res instanceof ResponseObject) {
          //Todo: Proper user friendly response handling using floating popups or something
          if (res.message) {
            console.log(res.message);
          } else if (res.error) {
            console.log("Error Response!");
            console.log(res.message);
          } else if (res.content) {
            props.responseHandler(res.content);
          } else {
            console.log("No response content!");
          }

          props.responseHandler();
        }
      })
      .catch((error) => {
        //Todo: Proper user friendly error handling using floating popups or something
        console.log("Edit Window Error!");
        console.log(error);
      });
  }

  return (
    <div>
      <h1>{props.windowDescription.title}</h1>
      <form onSubmit={submitHandler}></form>
    </div>
  );
}
