import * as signalR from '@microsoft/signalr';

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;
const RESERVATION_EVENT_NAME = 'ReservationEvent';

const hubUrl = `${apiBaseUrl}/api`;
console.log(`hub url: ${hubUrl}`);

export function createHubConnection(): signalR.HubConnection {
  console.log('Creating SignalR HubConnection...');
  const connection: signalR.HubConnection = new signalR.HubConnectionBuilder()
    .withAutomaticReconnect()
    .withUrl(hubUrl)
    .configureLogging(signalR.LogLevel.Information)
    .build();

  connection.onclose(() => console.log('SignalR disconnected.'));

  return connection;
}

var timeout: any = undefined;
var onConnectedCallback: (connectionId: string) => void = () => {};

function start(): any {
  if (connectionRef) {
    if (connectionRef.state !== signalR.HubConnectionState.Disconnected) return;
    console.log(`SignalR connecting [${connectionRef.state}]...`);
    connectionRef
      .start()
      .then(() => {
        signalRState.connectionId = connectionRef!.connectionId;
        onConnectedCallback(signalRState.connectionId!);
        console.log('SignalR connected.');
      })
      .catch((err: Error) => {
        console.error(err);
        timeout = setTimeout(start, 3000);
      });
  }
}

function stop() {
  if (timeout) clearTimeout(timeout);
  connectionRef?.stop();
}

var connectionRef: signalR.HubConnection | undefined = undefined;
if (!connectionRef) {
  connectionRef = createHubConnection();
}

export interface SignalRState {
  onReservationEvent: (callback: (...args: any[]) => any) => void;
  offReservationEvent: () => void;
  sendReservationEventAck: (invocationId: string, eventId: string) => Promise<any>;
  start: () => void;
  stop: () => void;
  onConnected: (callback: (connectionId: string) => void) => void;
  connectionId: string | null;
}

export const signalRState: SignalRState = {

  start: start,

  stop: stop,

  onReservationEvent: (callback: (...args: any[]) => any) => {
    connectionRef!.on(RESERVATION_EVENT_NAME, callback);
  },

  offReservationEvent: () => {
    connectionRef!.off(RESERVATION_EVENT_NAME);
  },

  sendReservationEventAck: (invocationId: string, eventId: string): Promise<any> => {
    return connectionRef!.invoke("ReservationEventAck", invocationId, eventId)
      .then(() => console.log('ReservationEventAck message sent.', { invocationId, eventId }))
      .catch(err => console.error("Failed broadcast", err));
  },

  onConnected: (callback: (connectionId: string) => void) => {
    onConnectedCallback = callback;
    if(!!signalRState.connectionId) {
      onConnectedCallback(signalRState.connectionId);
    }
  },

  connectionId: <string | null> null
};

// export const SignalRStateContext = createContext<SignalRState>(signalRState);