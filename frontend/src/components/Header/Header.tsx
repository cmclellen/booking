import { FC } from 'react';
import { HeaderWrapper } from './Header.styled';
import styled from 'styled-components';

const Brand = styled.a`
   font-family: RubikDoodleShadow, serif;
`;

const AppTitle = styled.small`
   font-family: CarterOne, serif;
`;

interface HeaderProps { }

const Header: FC<HeaderProps> = () => (
   <HeaderWrapper>
      <nav className="navbar navbar-expand-lg navbar-light bg-light">
         <div className="container">
            <Brand className="navbar-brand fs-1" href="https://cmclellen.github.io/">Craig McLellen <AppTitle className="fs-4">Reservation System</AppTitle></Brand>
            <button className="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav" aria-controls="navbarNav" aria-expanded="false" aria-label="Toggle navigation">
               <span className="navbar-toggler-icon"></span>
            </button>
         </div>
      </nav>
   </HeaderWrapper>
);

export default Header;
