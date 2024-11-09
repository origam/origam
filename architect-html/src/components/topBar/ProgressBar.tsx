import S from "src/components/topBar/ProgressBar.module.scss";

export const ProgressBar: React.FC<{
  isLoading: boolean;
}> = (props) => {
  return (
    <>
      {(props.isLoading || window.localStorage.getItem("debugKeepProgressIndicatorsOn")) && (
        <div className={S.progressIndicator}>
          <div className={S.indefinite}/>
        </div>
      )}
    </>
  );
}