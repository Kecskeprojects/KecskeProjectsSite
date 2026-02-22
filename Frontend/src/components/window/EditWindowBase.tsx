import type { JSX } from "react";
import React from "react";
import type IEditWindowBaseProps from "../../interface/IEditWIndowBaseProps";
import type IInputForEdit from "../../interface/IInputForEdit";
import type IWindowDescription from "../../interface/IWindowDescription";
import ResponseObject from "../../models/ResponseObject";
import EnvironmentTools from "../../tools/EnvironmentTools";
import LogTools from "../../tools/LogTools";
import InputBase from "../input/InputBase";

export default function EditWindowBase<
  windowDescriptionType extends IWindowDescription,
>(props: IEditWindowBaseProps<windowDescriptionType>): JSX.Element {
  const windowDescription = new props.windowDescriptionClass();
  const editedItem = {} as any;

  function submitHandler(e: React.SubmitEvent<HTMLFormElement>): void {
    e.preventDefault();
    const formData = new FormData(e.currentTarget);

    if (!EnvironmentTools.IsProduction()) {
      LogTools.DebugLog("Edited Item:", formData);
    }

    windowDescription
      .serviceFunction(formData)
      .then((res) => {
        if (res instanceof ResponseObject) {
          if (res.message) {
            LogTools.setSuccessNotification(res.message);
          } else if (res.error) {
            LogTools.setErrorNotification(res.error);
          } else if (!res.content) {
            LogTools.setErrorNotification("Server error!");
          }
        }

        props.responseHandler(res);
      })
      .catch((error) => {
        LogTools.setErrorNotification(error?.message ?? error);
      });
  }

  function inputUpdatedHandler(key: string, value: string): void {
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

  const classNames =
    "editwindow-base" +
    (windowDescription.className ? ` ${windowDescription.className}` : "");

  return (
    <div className={classNames}>
      <h1>{windowDescription.title}</h1>
      <form id={windowDescription.title} onSubmit={submitHandler}>
        {windowDescription.inputArray.map((input, ind) => {
          return renderInput(input, ind);
        })}
      </form>
      <button type="submit" form={windowDescription.title}>
        {windowDescription.buttonText ?? "Submit"}
      </button>
    </div>
  );
}
