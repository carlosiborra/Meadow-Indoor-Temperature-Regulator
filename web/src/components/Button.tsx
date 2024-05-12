import React from 'react';
import { twMerge } from 'tailwind-merge';

const Button = ({ children, onClick, className }: { children: React.JSX.Element | string, onClick: Function, className?: string }) => {
  return (
    <button
      onClick={() => onClick()}
      className={twMerge(
        "bg-dark-secondary text-light-primary flex flex-row items-center justify-center",
        "px-3 py-2 rounded-md text-center hover:scale-[1.025] transition-all duration-200",
        "drop-shadow-md hover:drop-shadow-lg focus:drop-shadow-lg focus:outline-none ",
        className
      )}>
      {children}
    </button>
  );
};

export default Button;
