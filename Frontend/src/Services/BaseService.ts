import axios, { AxiosHeaders, type AxiosProgressEvent } from "axios";
import BackendServiceTools from "../tools/BackendServiceTools";
import EnvironmentTools from "../tools/EnvironmentTools";

export default class BaseService {
  static GetUserStateEndpoint: string = "/Account/GetLoggedInUser";

  static async Get(
    route: string,
    queryItems?: any,
    additionalHeaders?: AxiosHeaders
  ): Promise<any> {
    return BaseService.BaseFetch(
      "GET",
      route,
      queryItems,
      undefined,
      undefined,
      undefined,
      additionalHeaders
    );
  }

  static async Post(
    route: string,
    queryItems?: any,
    body?: FormData,
    onUploadProgress?: (progressEvent: AxiosProgressEvent) => void,
    onDownloadProgress?: (progressEvent: AxiosProgressEvent) => void,
    additionalHeaders?: AxiosHeaders
  ): Promise<any> {
    return BaseService.BaseFetch(
      "POST",
      route,
      queryItems,
      body,
      onUploadProgress,
      onDownloadProgress,
      additionalHeaders
    );
  }

  static async Put(
    route: string,
    queryItems?: any,
    body?: FormData,
    additionalHeaders?: AxiosHeaders
  ): Promise<any> {
    return BaseService.BaseFetch(
      "PUT",
      route,
      queryItems,
      body,
      undefined,
      undefined,
      additionalHeaders
    );
  }

  static async Delete(
    route: string,
    queryItems?: any,
    body?: FormData,
    additionalHeaders?: AxiosHeaders
  ): Promise<any> {
    return BaseService.BaseFetch(
      "DELETE",
      route,
      queryItems,
      body,
      undefined,
      undefined,
      additionalHeaders
    );
  }

  static async BaseFetch(
    method: string,
    route: string,
    queryItems?: any,
    body?: FormData,
    onUploadProgress?: (progressEvent: AxiosProgressEvent) => void,
    onDownloadProgress?: (progressEvent: AxiosProgressEvent) => void,
    additionalHeaders?: AxiosHeaders
  ): Promise<any> {
    if (!route.startsWith("/") && !route.startsWith("\\")) {
      route = "/" + route;
    }

    const query = BackendServiceTools.BuildQuery(queryItems);
    let response;
    try {
      response = await axios(
        `${EnvironmentTools.getBackendRoute()}${route}${query}`,
        {
          method: method,
          withCredentials: !EnvironmentTools.IsProduction(),
          headers: additionalHeaders,
          data: body,
          validateStatus: (status) => status < 400,
          onDownloadProgress: onDownloadProgress,
          onUploadProgress: onUploadProgress,
        }
      );
    } catch (error) {
      BaseService.ErrorHandling(route, error);
    }

    const responseBody = response?.data;

    if (!EnvironmentTools.IsProduction() && responseBody) {
      console.log(responseBody);
    }

    return responseBody;
  }

  static ErrorHandling(route: string, errorData: any): void {
    if (!EnvironmentTools.IsProduction()) {
      //console.log(errorData.toJSON());
      //console.log(errorData.config);
      if (errorData.response) {
        // The request was made and the server responded with a status code
        console.log("[Error] Response From Backend:");
        console.log(errorData.response.data);
        console.log(errorData.response.status);
      } else if (errorData.request) {
        // The request was made but no response was received
        // `error.request` is an instance of XMLHttpRequest
        console.log("[Error] No Response From Backend:");
        console.log(errorData.request);
      } else {
        // Something happened in setting up the request that triggered an Error
        console.log("[Error] Pre-Request Configuration Issue:");
        console.log(errorData.message);
      }
    }

    //Todo: Proper user friendly error handling using floating popups or something
    const status = errorData?.response?.status
      ? errorData?.response?.status
      : errorData?.request?.status;
    const errorMessage = errorData?.response?.data?.error;
    if (errorMessage) {
      console.log(`Status Code: ${status}\nError: ${errorMessage}`);
    }

    if (
      (status === 401 || status === 403) &&
      !route.startsWith(BaseService.GetUserStateEndpoint)
    ) {
      window.location.replace("/login");
    }
  }
}
