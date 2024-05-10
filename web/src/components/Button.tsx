import React from 'react';

const Button = ({ buttonText }: { buttonText: string }) => {
  return (
    <button className="relative inline-flex items-center justify-center p-0.5 mb-2 me-2 overflow-hidden text-sm font-medium text-gray-900 rounded-lg group bg-gradient-to-br from-fountain-blue-500 to-fountain-blue-600 group-hover:from-fountain-blue-500 group-hover:to-fountain-blue-600 hover:text-white dark:text-white focus:ring-4 focus:outline-none focus:ring-cyan-200 dark:focus:ring-cyan-800">
      <span className="relative px-5 py-2.5 transition-all ease-in duration-75 bg-white dark:bg-gray-900 rounded-md group-hover:bg-opacity-0">
        {buttonText}
      </span>
    </button>
  );
};

export default Button;
