import { FC, useCallback, useEffect, useState } from 'react';
import { Icon, WorkflowWrapper } from './Workflow.styled';
import axios from 'axios';
import { sigR } from '../../signalr-context';
import { Form } from 'react-bootstrap';
import { v4 as uuid } from 'uuid';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faBed, faCar, faPersonWalking, faPlane } from '@fortawesome/free-solid-svg-icons';

interface IReservationEvent {
   message: string;
   type?: string;
}

interface WorkflowProps { }

const Workflow: FC<WorkflowProps> = () => {
   const [events, setEvents] = useState<Array<IReservationEvent>>([]);
   const [invocationId, setInvocationId] = useState<string>();
   const [simulateFailure, setSimulateFailure] = useState<string | undefined>(undefined);
   const [simulateFailureEnabled, setSimulateFailureEnabled] = useState<boolean>(false);
   const [canReserve, setCanReserve] = useState<boolean>(true);

   const onReservationEvent = async(message: string, type: string, inboundInvocationId: string, eventId: string) => {
      if (inboundInvocationId === invocationId) {
         addEvent({
            message,
            type
         });
      }
      setTimeout(async () => {
         await sigR.sendReservationEventAck(invocationId!, eventId)
      });
   };

   useEffect(() => {
      sigR.registerReservationEvent(onReservationEvent);

      return () => {
         sigR.unregisterReservationEvent();
      }
   });

   const handleBookHoliday = useCallback(async () => {
      var url = `${import.meta.env.VITE_API_BASE_URL}/api/Reservation_HttpStart`;
      console.log(`Invoking ${url}...`);
      var eventList = [{ message: `Initiating reservation...` }];
      setEvents(eventList);
      const connectionId = sigR.getConnectionId();
      console.log(`connectionId: ${connectionId}`);
      var id = uuid();
      setInvocationId(id);
      await axios.post(url, { connectionId, simulateFailure, id });
      eventList.push({ message: `Reservation initiated.` })
      setEvents(eventList);
   }, [setEvents, setInvocationId]);

   const addEvent = (ev: IReservationEvent) => {
      setTimeout(() => setEvents([...events, ev]));
   };

   function onSimulateFailureChanged(e: any) {
      var val = e.target.value;
      setSimulateFailure(val);
   }

   function onSimulateFailureEnabledChanged(e: any) {
      var val = e.target.checked;
      setSimulateFailureEnabled(val);
      if (!val) {
         setSimulateFailure(undefined);
      }
   }

   function getIcon(type?: string) {
      if (!type) return null;
      var attributes = undefined;
      switch (type) {
         case 'Car':
            attributes = {
               icon: faCar,
               colorClass: 'text-danger'
            };
            break;
         case 'Hotel':
            attributes = {
               icon: faBed,
               colorClass: 'text-primary'
            };
            break;
         case 'Flight':
            attributes = {
               icon: faPlane,
               colorClass: 'text-success'
            };
            break;
         default:
            throw new Error(`Unexpected type ${type}.`);
      }
      return (<Icon className="me-2"><FontAwesomeIcon icon={attributes.icon} className={attributes?.colorClass} /></Icon>);
   }

   return (
      <WorkflowWrapper>

         <div className="my-3">
            <p>Your next holiday is long overdue. Reserve your holiday below, and leave the flight, rental car and hotel reservations to us.</p>
            <button type="button" className="btn btn-outline-secondary" disabled={!canReserve} onClick={handleBookHoliday}>Reserve your holiday to Hawaii</button>
         </div>
         <Form>
            <Form.Check
               inline
               disabled={!canReserve}
               type='checkbox'
               label='Simulate failure with reservation'
               id={`simulate-failure-enabled`}
               name="simulateFailureEnabled"
               checked={simulateFailureEnabled}
               onChange={onSimulateFailureEnabledChanged}
            />

            <div className="d-inline-flex">
               {['Flight', 'Car', 'Hotel'].map((type, typeIndex) => (
                  <Form.Check                     
                     key={`type-${typeIndex}`}
                     disabled={!canReserve || !simulateFailureEnabled}
                     inline
                     type='radio'
                     label={type}
                     id={`simulate-failure-${type}`}
                     name="simulateFailure"
                     value={type}
                     checked={simulateFailure === type}
                     onChange={onSimulateFailureChanged}
                  />
               ))}
            </div>
         </Form>

         <hr className="border-bottom border-1 border-dark"></hr>

         <div className="d-md-inline-flex align-items-center">
            <h3 className="text-muted mb-0 mb-md-2">Event Log</h3>
            {!!invocationId && (<small className="text-muted ms-md-3">Invocation ID: {invocationId}</small>)}
         </div>

         {!!invocationId && (
            <div className="alert alert-warning alert-dismissible fade show" role="alert">
               The system has been snoozing...please allow a couple seconds for it to wake up <FontAwesomeIcon icon={faPersonWalking}></FontAwesomeIcon>
               <button type="button" className="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>)}

         {events.length == 0 &&
            <div>No event(s) have been logged just yet.</div>
         }
         <ol className="list-group list-group-numbered">
            {events.map((step, index) => (
               <li key={`event-${index}`} className="list-group-item d-flex justify-content-between p-3 align-items-center">
                  <div className="ms-2 me-auto d-flex">
                     {getIcon(step.type)}
                     <div className="fw-bold">{step.message}</div>
                  </div>
               </li>)
            )}
         </ol>

      </WorkflowWrapper>
   );
}

export default Workflow;
