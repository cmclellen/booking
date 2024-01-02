import React, { FC } from 'react';
import { DesignPageWrapper } from './DesignPage.styled';
import designsvg from '../../assets/design.svg';

interface DesignPageProps {}

const DesignPage: FC<DesignPageProps> = () => (
 <DesignPageWrapper>
   <div className="container">
      <h3>Overview</h3>
      <p>
         This is a reservation system designed to take the hassle out of reserving flights, rental cars and hotels. The process is initiated by a click of a button on the web page, after which the system orchestrates the 
         reservation of a flight, car rental and hotel for the user.
      </p>
      <p>
         The UI is a React SPA that is an interface to a Durable Function that handles the orchestration of the reservations. Once the user initates the reservation via a button click, the UI makes an HTTP POST request to an <i>HttpTrigger</i> Azure Function, which in turn begins the orchestration.
         All events are reported back to the UI via an Azure SignalR service that the Durable Function interacts with.
      </p>
      <h3>Architecture Diagram</h3>
      <img src={designsvg} alt="Design" />
   </div>    
 </DesignPageWrapper>
);

export default DesignPage;
