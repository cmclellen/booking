import { FC, useState } from 'react';
import { WorkflowWrapper } from './Workflow.styled';
import axios from 'axios';

interface WorkflowProps { }

const initiateBooking = async () => {
   
   const response =
     await axios.post("/api/Reservation_HttpStart")
   console.log(response.data)
}

const Workflow: FC<WorkflowProps> = () => {
   const [events] = useState([]);

   const handleBookHoliday = async () => { 
      initiateBooking().then(() => {
         console.log('Initiated.');
      });
   };

   return (
      <WorkflowWrapper>

         <div className="my-3">
            <p>Your next holiday is long overdue. Book below, and leave the flight, car rental and hotel bookings to us.</p>
            <button type="button" className="btn btn-outline-secondary" onClick={handleBookHoliday}>Book your holiday to Hawaii</button>
         </div>

         <hr className="border-bottom border-1 border-dark"></hr>

         <h3 className="text-muted">The Event Log</h3>
         {events.length == 0 &&
            <div>No event(s) have been logged just yet.</div>
         }
         <ol className="list-group list-group-numbered">
            {events.map((step, index) => (
               <li className="list-group-item d-flex justify-content-between align-items-start">
                  <div className="ms-2 me-auto">
                     <div className="fw-bold">Subheading {step}</div>
                     Cras justo odio {index}
                  </div>
               </li>)
            )}
         </ol>

      </WorkflowWrapper>
   );
}

export default Workflow;
