import { FC, useEffect, useState } from 'react';
import { WorkflowWrapper } from './Workflow.styled';
import axios from 'axios';
import connectionRef from '../../signalr-context';
import { Form } from 'react-bootstrap';
import { v4 as uuid } from 'uuid';

interface WorkflowProps { }

const Workflow: FC<WorkflowProps> = () => {
   const [events, setEvents] = useState<Array<string>>([]);
   const [invocationId, setInvocationId] = useState<string>();
   const [simulateFailure, setSimulateFailure] = useState<string | undefined>(undefined);
   const [simulateFailureEnabled, setSimulateFailureEnabled] = useState<boolean>(false);

   useEffect(() => {
      connectionRef!.on('ReservationEvent', (message: string, id: string) => {
         if(id === invocationId) {
            addEvent(message);
         }
      });

      return () => {
         connectionRef!.off('ReservationEvent');
      }
   });

   const handleBookHoliday = async () => {
      var url = `${import.meta.env.VITE_API_BASE_URL}/api/Reservation_HttpStart`;
      console.log(`Invoking ${url}...`);
      var eventList = [`Initiating reservation...`];
      setEvents(eventList);
      const connectionId = connectionRef?.connectionId;
      var id = uuid();
      setInvocationId(id);
      console.log(`started: ${id}`);
      await axios.post(url, { connectionId, simulateFailure, id });
      setEvents([...eventList, `Reservation initiated.`]);
   };

   const addEvent = (message: string) => {
      setEvents([...events, message]);
   };

   function onSimulateFailureChanged(e: any) {
      var val = e.target.value;
      setSimulateFailure(val);
   }

   function onSimulateFailureEnabledChanged(e: any) {
      var val = e.target.checked;
      setSimulateFailureEnabled(val);
      if(!val) {
         setSimulateFailure(undefined);
      }
   }

   return (
      <WorkflowWrapper>

         <div className="my-3">
            <p>Your next holiday is long overdue. Reserve your holiday below, and leave the flight, rental car and hotel reservations to us.</p>
            <button type="button" className="btn btn-outline-secondary" onClick={handleBookHoliday}>Reserve your holiday to Hawaii</button>
         </div>
         <Form>
            <Form.Check
               inline
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
                  disabled={!simulateFailureEnabled}
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

         <h3 className="text-muted">Event Log</h3>
         {events.length == 0 &&
            <div>No event(s) have been logged just yet.</div>
         }
         <ol className="list-group list-group-numbered">
            {events.map((step, index) => (
               <li key={`event-${index}`} className="list-group-item d-flex justify-content-between align-items-start">
                  <div className="ms-2 me-auto">
                     <div className="fw-bold">{step}</div>
                  </div>
               </li>)
            )}
         </ol>

      </WorkflowWrapper>
   );
}

export default Workflow;
