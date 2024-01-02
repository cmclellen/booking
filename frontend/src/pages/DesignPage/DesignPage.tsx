import { FC } from 'react';
import { DesignPageWrapper, Heading } from './DesignPage.styled';
import designsvg from '../../assets/design.svg';
import sequencesvg from '../../assets/sequence.svg';
import { faTriangleExclamation } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';

interface DesignPageProps { }

const DesignPage: FC<DesignPageProps> = () => (
   <DesignPageWrapper>
      <div className="container">
         <Heading>Overview</Heading>
         <p>
            This is a reservation system designed to take the hassle out of reserving flights, rental cars and hotels. The process is initiated by a click of a button on the web page, after which the system orchestrates the
            reservation of a flight, car rental and hotel for the user.
         </p>
         <p>
            The UI is a React SPA that interfaces with an Azure Function App. The user initiates a reservation via a button click on the UI, which calls through to an <i>Http</i> triggered Azure Function.
            This function then initates the orchestration by calling a Durable Function. All events throughout the orchestration are reported back to the UI via an Azure SignalR service that the Durable Function interacts with.
            The user has the option to simulate a failure at any one of the reservation steps, which would then demonstrate compensating actions cancelling any prior reservations, ensuring the reservation is not left in an inconsistent state.
         </p>
         <div className="alert alert-warning" role="alert">
            <FontAwesomeIcon icon={faTriangleExclamation}></FontAwesomeIcon> In a real world scenario, other solutions could be incorporated to ensure the entire reservation eventually completes successfully without the reservation being
            cancelled, eg. retries, etc. This example was just to demo sagas using Durable functions and compensating actions.
         </div>
         <Heading>Architecture Diagram</Heading>
         <img src={designsvg} alt="Architecture Diagram" className="img-fluid" />
         <Heading>Sequence Diagram</Heading>
         <img src={sequencesvg} alt="Sequence Diagram" className="img-fluid" />
      </div>
   </DesignPageWrapper>
);

export default DesignPage;
