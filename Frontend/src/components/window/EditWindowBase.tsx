import type { JSX } from "react";
import React from "react";
import type EditWindowBaseProps from "../../interface/IEditWIndowBaseProps";
import type IInputForEdit from "../../interface/IInputForEdit";
import type IWindowDescription from "../../interface/IWindowDescription";
import ResponseObject from "../../models/ResponseObject";
import EnvironmentTools from "../../tools/EnvironmentTools";
import LogTools from "../../tools/LogTools";
import InputBase from "../input/InputBase";

export default function EditWindowBase<
  windowDescriptionType extends IWindowDescription
>(props: EditWindowBaseProps<windowDescriptionType>): JSX.Element {
  const windowDescription = new props.windowDescriptionClass();
  const editedItem = {} as any;

  function submitHandler(e: React.FormEvent<HTMLFormElement>): void {
    e.preventDefault();
    const formData = new FormData(e.currentTarget);

    if (!EnvironmentTools.IsProduction()) {
      LogTools.DebugLog("Edited Item:", formData);
    }

    windowDescription
      .serviceFunction(formData)
      .then((res) => {
        if (res instanceof ResponseObject) {
          //Todo: Proper user friendly response handling using floating popups or something
          if (res.message) {
            console.log(res.message);
          } else if (res.error) {
            console.log("Error Response!");
            console.log(res.message);
          } else if (!res.content) {
            console.log("No response content!");
          }
        }

        props.responseHandler(res);
      })
      .catch((error) => {
        //Todo: Proper user friendly error handling using floating popups or something
        console.log("Edit Window Error!");
        console.log(error);
      });
  }

  function inputUpdatedHandler(e: React.FormEvent<HTMLInputElement>): void {
    const key = e.currentTarget.name;
    const value = e.currentTarget.value;

    editedItem[key] = value;
  }

  function renderInput(input: IInputForEdit, ind: number): JSX.Element {
    return (
      <InputBase
        {...input}
        updatedHandler={inputUpdatedHandler}
        editedItem={editedItem}
        key={ind}
      />
    );
  }

  //Todo: hasCloseFunctionality is not implemented, as well as the close functionality itself

  return (
    <div
      className={
        "" +
        (windowDescription.className ? ` ${windowDescription.className}` : "")
      }
    >
      <h1>{windowDescription.title}</h1>
      <form id={windowDescription.title} onSubmit={submitHandler}>
        {windowDescription.inputArray.map((input, ind) => {
          return renderInput(input, ind);
        })}
        <button type="submit">
          {windowDescription.buttonText ?? "Submit"}
        </button>
      </form>
    </div>
  );
}
