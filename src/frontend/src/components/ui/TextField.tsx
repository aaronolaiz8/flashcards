import type { InputHTMLAttributes } from "react";
import { forwardRef } from "react";
import { cn } from "../../lib/cn";

interface TextFieldProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  error?: string;
}

export const TextField = forwardRef<HTMLInputElement, TextFieldProps>(
  ({ label, error, className, ...props }, ref) => (
    <label className="block">
      <span className="mb-1.5 block text-sm font-medium text-gray-700">{label}</span>
      <input
        ref={ref}
        className={cn(
          "w-full rounded-lg border border-border-soft bg-white px-3 py-2 text-sm text-gray-800 outline-none transition-colors focus:border-brand-500 focus:ring-2 focus:ring-brand-100",
          error && "border-danger-500 focus:border-danger-500 focus:ring-danger-100",
          className,
        )}
        {...props}
      />
      {error && <span className="mt-1 block text-xs text-danger-600">{error}</span>}
    </label>
  ),
);
TextField.displayName = "TextField";
