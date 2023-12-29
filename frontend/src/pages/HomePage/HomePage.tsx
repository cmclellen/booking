import { FC } from 'react';
import { HomePageWrapper } from './HomePage.styled';
import Workflow from '../../components/Workflow/Workflow';

interface HomePageProps { }

const HomePage: FC<HomePageProps> = () => (
   <HomePageWrapper>
      <div className="container">
         <Workflow />
      </div>
   </HomePageWrapper>
);

export default HomePage;


