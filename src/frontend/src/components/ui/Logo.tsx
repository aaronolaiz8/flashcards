import { cn } from "../../lib/cn";

/** Retainica brand mark — a Garamond capital "R" (EB Garamond) in white on the brand-purple tile. */
export function LogoMark({ className }: { className?: string }) {
  return (
    <svg viewBox="0 0 64 64" className={cn("h-8 w-8", className)} aria-hidden="true">
      <rect width="64" height="64" rx="16" fill="var(--color-brand-600)" />
      <path
        transform="translate(10.48 50.44) scale(0.058 -0.058)"
        fill="#ffffff"
        d="M637 -21Q609 -21 584.0 -16.5Q559 -12 538.0 0.0Q517 12 499 33Q475 63 453.0 90.5Q431 118 406.5 154.0Q382 190 347 246Q332 271 304 285Q282 296 260.5 300.5Q239 305 225 306Q217 307 213.5 301.0Q210 295 210 286V110Q210 78 225.0 54.5Q240 31 277 24Q294 21 302.0 17.0Q310 13 310 6Q310 0 302.0 -2.5Q294 -5 281 -5Q254 -5 239.5 -3.0Q225 -1 213.5 0.5Q202 2 183 2Q158 2 138.5 0.5Q119 -1 99.5 -3.0Q80 -5 53 -5Q40 -5 32.0 -2.5Q24 0 24 6Q24 18 57 24Q95 31 112.5 47.0Q130 63 130 100V526Q130 564 126.5 583.5Q123 603 110.5 611.0Q98 619 69 622Q52 624 44.0 629.5Q36 635 36 642Q36 648 44.0 650.5Q52 653 65 653Q92 653 105.5 653.0Q119 653 131.0 652.5Q143 652 163 652Q182 652 197.0 653.5Q212 655 230.5 656.0Q249 657 278 657Q388 657 445.5 616.0Q503 575 503 493Q503 448 485.0 415.5Q467 383 440.0 361.0Q413 339 385 325Q381 324 381.5 320.5Q382 317 385 312Q416 265 444.5 223.0Q473 181 507.0 141.0Q541 101 588 58Q614 35 641.0 30.0Q668 25 703 25Q718 25 718 14Q718 -1 703.0 -8.5Q688 -16 669.0 -18.5Q650 -21 637 -21ZM263 329Q309 329 342.5 350.0Q376 371 394.5 405.0Q413 439 413 477Q413 518 402.5 552.0Q392 586 360.5 606.5Q329 627 266 627Q234 627 224.0 606.5Q214 586 212 534Q211 509 210.5 466.5Q210 424 210 364Q210 339 224.0 334.0Q238 329 263 329Z"
      />
    </svg>
  );
}

/** Full Retainica lockup: mark + wordmark. */
export function Logo({ className, markClassName }: { className?: string; markClassName?: string }) {
  return (
    <div className={cn("flex items-center gap-2", className)}>
      <LogoMark className={markClassName} />
      <span className="text-lg font-semibold tracking-tight text-text-heading">Retainica</span>
    </div>
  );
}
