import { FC } from 'react';
import { FooterWrapper } from './Footer.styled';
import styled from 'styled-components';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faAt, faEnvelope } from '@fortawesome/free-solid-svg-icons';

const Inner = styled.div`
   background-color: black;
`;

interface FooterProps {}

const Footer: FC<FooterProps> = () => (
 <FooterWrapper data-testid="Footer">
    <Inner className="text-center mt-3 py-3 text-white">&copy; 2023 <FontAwesomeIcon icon={faEnvelope}></FontAwesomeIcon> <span className="fst-italic">cmclellen<FontAwesomeIcon className="px-1" size="sm" icon={faAt}></FontAwesomeIcon>gmail.com</span></Inner>
 </FooterWrapper>
);

export default Footer;
