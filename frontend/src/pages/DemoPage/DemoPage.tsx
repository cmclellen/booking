import { FC } from 'react';
import { DemoPageWrapper } from './DemoPage.styled';
import Workflow from '../../components/Workflow/Workflow';

interface DemoPageProps { }

const DemoPage: FC<DemoPageProps> = () => (
   <DemoPageWrapper>
      <div className="container">
         <Workflow />
      </div>
   </DemoPageWrapper>
);

export default DemoPage;
