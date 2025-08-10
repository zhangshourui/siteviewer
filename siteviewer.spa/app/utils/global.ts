
interface ApiRequestOption {
    /**
     * Response wait tolerance. If there is no response within this time, 
     * a loading box will be displayed.
     * 0 means always display, -1 means do not display. Unit: milliseconds 
     */
    tolerance: number;

    /**
     * Whether to display errors via toast when they occur. Default is to display (false),
     * silent mode (true) means no toast will be shown.
     */
    silence: boolean;

    noErrorToast: boolean;

    headers: any[]; 
}

const defaultHttpApiOptions: ApiRequestOption = {
    tolerance: 500,
    silence: false,
    noErrorToast: false,
    headers: []
};

interface MsgRespBase {
    Result: number //, Action: string, Path: string,
    ErrorMsg?: string;
    Data?: any;
}


// API base URL for production environment
let apiBase = 'http://192.168.1.140:8082/api';

// API base URL for development environment
if (process.env.NODE_ENV === 'development') {
    apiBase = 'http://localhost:5199/api';
}

export {
    defaultHttpApiOptions,
    type ApiRequestOption,
    type MsgRespBase,
    apiBase
};